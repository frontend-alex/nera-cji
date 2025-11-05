namespace nera_cji.Models;

using System;
using System.Text.Json.Serialization;

public class User {
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string NormalizedEmail => Email.ToUpperInvariant();

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}


