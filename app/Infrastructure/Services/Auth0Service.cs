namespace nera_cji.Services;

using System.Text.Json;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Auth0.Core.Exceptions;
using nera_cji.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
}

