namespace nera_cji.Models;

using System;
using System.Text.Json.Serialization;

public class User {
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string email { get; set; } = string.Empty;
    public bool? is_active { get; set; } = true;
    public int? department_id { get; set; }
    public bool is_admin { get; set; } = false;
    public DateTime created_at { get; set; } = DateTime.UtcNow;
    public DateTime? updated_at { get; set; }
    [JsonIgnore]
    public string NormalizedEmail => email.ToUpperInvariant();

    public string password_hash { get; set; } = string.Empty;

    
}


