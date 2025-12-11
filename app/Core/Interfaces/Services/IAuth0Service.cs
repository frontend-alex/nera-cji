namespace nera_cji.Interfaces.Services;

public interface IAuth0Service {
    Task<Auth0LoginResult> LoginAsync(string email, string password);
    Task<Auth0SignupResult> SignupAsync(string email, string password, string fullName);
    Task<Auth0UserInfo?> GetUserInfoAsync(string accessToken);
    Task<bool> BlockUserAsync(string email, bool block);
    Task<string?> GetUserIdByEmailAsync(string email);
    Task<bool> UpdatePasswordAsync(string email, string newPassword);
}

public class Auth0LoginResult {
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? IdToken { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
}

public class Auth0SignupResult {
    public bool Success { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? ErrorMessage { get; set; }
}

public class Auth0UserInfo {
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Sub { get; set; }
}

