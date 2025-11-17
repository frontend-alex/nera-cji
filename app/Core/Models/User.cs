namespace nera_cji.Models;

using System;
using System.Text.Json.Serialization;

public class User {
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string email { get; set; } = string.Empty;
    public string is_active { get; set; } = string.Empty;
    public string department_id { get; set; } = string.Empty;
    public bool is_admin { get; set; } = false;

    [JsonIgnore]
    public string NormalizedEmail => email.ToUpperInvariant();

    public string password_hash { get; set; } = string.Empty;

    public DateTime created_at { get; set; } = DateTime.UtcNow;
}


