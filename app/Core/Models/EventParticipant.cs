namespace nera_cji.Models;

using System;

public class EventParticipant
{
    public int Id { get; set; }

    public int Event_Id { get; set; }

    public int User_Id { get; set; }

    public DateTime Registered_At { get; set; } = DateTime.UtcNow;

    public string? Status { get; set; }
}
