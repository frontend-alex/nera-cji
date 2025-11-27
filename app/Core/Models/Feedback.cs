namespace nera_cji.Models;

using System;

public class Feedback
{
    public int Id { get; set; }

    public int Event_Id { get; set; }

    public int User_Id { get; set; }

    public int? Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime Submitted_At { get; set; } = DateTime.UtcNow;
}
