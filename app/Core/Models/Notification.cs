namespace nera_cji.Models;

using System;
using System.Text.Json.Serialization;

public class Notification
{
    public int Id { get; set; }

    public int User_Id { get; set; }

    public int? Event_Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool Is_Read { get; set; } = false;

    public DateTime Created_At { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public bool IsNew => !Is_Read && (DateTime.UtcNow - Created_At).TotalHours < 24;
}
