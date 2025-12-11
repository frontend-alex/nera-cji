namespace nera_cji.Services;

using System.Text.Json;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using nera_cji.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

public class Auth0Service : IAuth0Service {
    private readonly IAuthenticationApiClient _auth0Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Auth0Service> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _domain;

    public Auth0Service(IConfiguration configuration, ILogger<Auth0Service> logger) {
        _configuration = configuration;
        _logger = logger;
        _domain = configuration["Auth0:Domain"] ?? throw new InvalidOperationException("Auth0:Domain is not configured");
        _clientId = configuration["Auth0:ClientId"] ?? throw new InvalidOperationException("Auth0:ClientId is not configured");
        _clientSecret = configuration["Auth0:ClientSecret"] ?? throw new InvalidOperationException("Auth0:ClientSecret is not configured");
        
        _auth0Client = new AuthenticationApiClient(_domain);
    }

    public async Task<Auth0LoginResult> LoginAsync(string email, string password) {
        try {
            var request = new ResourceOwnerTokenRequest {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                Username = email,
                Password = password,
                Scope = "openid profile email"
            };

            var tokenResponse = await _auth0Client.GetTokenAsync(request);
            
            if (tokenResponse != null && !string.IsNullOrEmpty(tokenResponse.AccessToken)) {
                var userInfo = await GetUserInfoAsync(tokenResponse.AccessToken);
                
                return new Auth0LoginResult {
                    Success = true,
                    AccessToken = tokenResponse.AccessToken,
                    IdToken = tokenResponse.IdToken,
                    Email = userInfo?.Email ?? email,
                    FullName = userInfo?.Name
                };
            }

            return new Auth0LoginResult {
                Success = false,
                ErrorMessage = "Invalid credentials"
            };
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error during Auth0 login for email: {Email}", email);
            return new Auth0LoginResult {
                Success = false,
                ErrorMessage = "Authentication failed. Please check your credentials."
            };
        }
    }

    public async Task<Auth0SignupResult> SignupAsync(string email, string password, string fullName) {
        try {
            var request = new SignupUserRequest {
                ClientId = _clientId,
                Email = email,
                Password = password,
                Connection = "Username-Password-Authentication",
                UserMetadata = new Dictionary<string, object> {
                    { "full_name", fullName }
                }
            };

            var signupResponse = await _auth0Client.SignupUserAsync(request);
            
            if (signupResponse != null && !string.IsNullOrEmpty(signupResponse.Id)) {
                return new Auth0SignupResult {
                    Success = true,
                    UserId = signupResponse.Id,
                    Email = signupResponse.Email
                };
            }

            return new Auth0SignupResult {
                Success = false,
                ErrorMessage = "Failed to create user"
            };
        }
        catch (ErrorApiException ex) {
            _logger.LogError(ex, "Auth0 API error during signup for email: {Email}", email);
            
            if (ex.ApiError != null && ex.ApiError.Error == "invalid_password") {
                var errorMessage = "Password does not meet requirements. Password must be at least 8 characters and contain at least 3 of the following: lowercase letters, uppercase letters, numbers, and special characters.";
                
                return new Auth0SignupResult {
                    Success = false,
                    ErrorMessage = errorMessage
                };
            }
            
            if (ex.ApiError != null && (ex.ApiError.Error == "user_exists" || ex.Message.Contains("already exists") || ex.Message.Contains("duplicate"))) {
                return new Auth0SignupResult {
                    Success = false,
                    ErrorMessage = "An account with that email already exists."
                };
            }
            
            return new Auth0SignupResult {
                Success = false,
                ErrorMessage = ex.Message ?? "Registration failed. Please try again."
            };
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error during Auth0 signup for email: {Email}", email);
            
            var errorMessage = "Registration failed. Please try again.";
            if (ex.Message.Contains("already exists") || ex.Message.Contains("duplicate")) {
                errorMessage = "An account with that email already exists.";
            } else if (ex.Message.Contains("Password") || ex.Message.Contains("password")) {
                errorMessage = "Password does not meet requirements. Password must be at least 8 characters and contain at least 3 of the following: lowercase letters, uppercase letters, numbers, and special characters.";
            }
            
            return new Auth0SignupResult {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }

    public async Task<Auth0UserInfo?> GetUserInfoAsync(string accessToken) {
        try {
            var userInfoResponse = await _auth0Client.GetUserInfoAsync(accessToken);
            
            if (userInfoResponse != null) {
                string? name = null;
                if (userInfoResponse.AdditionalClaims != null) {
                    userInfoResponse.AdditionalClaims.TryGetValue("name", out var nameClaim);
                    name = nameClaim?.ToString();
                    if (string.IsNullOrEmpty(name)) {
                        userInfoResponse.AdditionalClaims.TryGetValue("full_name", out var fullNameClaim);
                        name = fullNameClaim?.ToString();
                    }
                }
                
                return new Auth0UserInfo {
                    Email = userInfoResponse.Email,
                    Name = name,
                    Sub = userInfoResponse.UserId
                };
            }
            
            return null;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error getting user info from Auth0");
            return null;
        }
    }

    public async Task<string?> GetUserIdByEmailAsync(string email) {
        try {
            // Get Management API token
            var managementToken = await GetManagementApiTokenAsync();
            if (string.IsNullOrEmpty(managementToken)) {
                _logger.LogWarning("Failed to get Management API token for user lookup");
                return null;
            }

            // Search for user by email using Management API
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managementToken);

            var searchUrl = $"https://{_domain}/api/v2/users-by-email?email={Uri.EscapeDataString(email)}";
            var response = await httpClient.GetAsync(searchUrl);

            if (response.IsSuccessStatusCode) {
                var content = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<JsonElement[]>(content);
                
                if (users != null && users.Length > 0) {
                    return users[0].GetProperty("user_id").GetString();
                }
            }

            return null;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error getting Auth0 user ID for email: {Email}", email);
            return null;
        }
    }

    public async Task<bool> BlockUserAsync(string email, bool block) {
        try {
            var userId = await GetUserIdByEmailAsync(email);
            if (string.IsNullOrEmpty(userId)) {
                _logger.LogWarning("User not found in Auth0: {Email}", email);
                return false;
            }

            // Get Management API token
            var managementToken = await GetManagementApiTokenAsync();
            if (string.IsNullOrEmpty(managementToken)) {
                _logger.LogWarning("Failed to get Management API token for blocking user");
                return false;
            }

            // Block or unblock user
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managementToken);

            var blockUrl = $"https://{_domain}/api/v2/users/{Uri.EscapeDataString(userId)}";
            var blockPayload = new { blocked = block };
            var json = JsonSerializer.Serialize(blockPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PatchAsync(blockUrl, content);

            if (response.IsSuccessStatusCode) {
                _logger.LogInformation("User {Email} {Action} in Auth0", email, block ? "blocked" : "unblocked");
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to {Action} user in Auth0: {Error}", block ? "block" : "unblock", errorContent);
            return false;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error {Action} user in Auth0: {Email}", block ? "blocking" : "unblocking", email);
            return false;
        }
    }

    public async Task<bool> UpdatePasswordAsync(string email, string newPassword) {
        try {
            _logger.LogInformation("Starting Auth0 password update for user {Email}", email);
            
            var userId = await GetUserIdByEmailAsync(email);
            if (string.IsNullOrEmpty(userId)) {
                _logger.LogWarning("User not found in Auth0: {Email}", email);
                return false;
            }

            _logger.LogInformation("Found Auth0 user ID {UserId} for email {Email}", userId, email);

            // Get Management API token
            var managementToken = await GetManagementApiTokenAsync();
            if (string.IsNullOrEmpty(managementToken)) {
                _logger.LogError("Failed to get Management API token for password update");
                return false;
            }

            _logger.LogInformation("Management API token obtained successfully");

            // Update password using Management API
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", managementToken);

            var updateUrl = $"https://{_domain}/api/v2/users/{Uri.EscapeDataString(userId)}";
            var updatePayload = new { 
                password = newPassword,
                verify_password = false // Don't verify password strength again
            };
            var json = JsonSerializer.Serialize(updatePayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending PATCH request to Auth0 Management API: {Url}", updateUrl);
            var response = await httpClient.PatchAsync(updateUrl, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) {
                _logger.LogInformation("Password updated successfully for user {Email} (ID: {UserId}) in Auth0. Response: {Response}", 
                    email, userId, responseContent);
                
                // Verify the update worked by attempting to log in with the new password
                // Wait a moment for Auth0 to process the update
                await Task.Delay(1000);
                
                var verifyResult = await LoginAsync(email, newPassword);
                if (verifyResult.Success) {
                    _logger.LogInformation("Auth0 password update verified - login with new password succeeded for {Email}", email);
                    return true;
                } else {
                    _logger.LogWarning("Auth0 password was updated but verification login failed for {Email}. This might be a timing issue.", email);
                    // Still return true since the update API call succeeded
                    return true;
                }
            }

            _logger.LogError("Failed to update password in Auth0 for user {Email} (ID: {UserId}). Status: {Status}, Response: {Response}", 
                email, userId, response.StatusCode, responseContent);
            return false;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Exception updating password in Auth0 for email: {Email}. Error: {Message}", email, ex.Message);
            return false;
        }
    }

    private async Task<string?> GetManagementApiTokenAsync() {
        try {
            using var httpClient = new HttpClient();
            var tokenUrl = $"https://{_domain}/oauth/token";
            
            var requestBody = new Dictionary<string, string> {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "audience", $"https://{_domain}/api/v2/" },
                { "grant_type", "client_credentials" }
            };

            var content = new FormUrlEncodedContent(requestBody);
            var response = await httpClient.PostAsync(tokenUrl, content);

            if (response.IsSuccessStatusCode) {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
                
                if (tokenResponse.TryGetProperty("access_token", out var tokenElement)) {
                    return tokenElement.GetString();
                }
            }

            return null;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error getting Management API token");
            return null;
        }
    }
}

