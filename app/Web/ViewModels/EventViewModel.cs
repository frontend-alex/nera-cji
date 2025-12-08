namespace nera_cji.ViewModels;

using System.ComponentModel.DataAnnotations;

public class CreateEventViewModel {
    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [MaxLength(255)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public TimeSpan Time { get; set; }

    public int? MaxParticipants { get; set; }

    public string? Status { get; set; }

    public decimal Event_Cost { get; set; }
}

