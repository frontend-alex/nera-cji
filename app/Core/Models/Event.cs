namespace nera_cji.Models;

using System;
using System.Text.Json.Serialization;

public class Event
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateTime Start_Time { get; set; }

    public DateTime? End_Time { get; set; }

    public int Created_By { get; set; }

    public int? Max_Participants { get; set; }

    public string? Status { get; set; }

    public DateTime Created_At { get; set; } = DateTime.UtcNow;

    public DateTime? Updated_At { get; set; }

    [JsonIgnore]
    public bool IsUpcoming => Start_Time > DateTime.UtcNow;
}
