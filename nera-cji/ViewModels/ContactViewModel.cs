namespace nera_cji.ViewModels;

using System.ComponentModel.DataAnnotations;

public enum ContactTopic {
    [Display(Name = "General inquiries")]
    General,

    [Display(Name = "Media and press")]
    Media,

    [Display(Name = "Partnership opportunities")]
    Partnerships,

    [Display(Name = "Technology & platform support")]
    TechnicalSupport,

    [Display(Name = "Careers and recruitment")]
    Careers
}

public class ContactFormViewModel {
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(120)]
    public string? Organization { get; set; }

    [Display(Name = "How can we help?")]
    public ContactTopic Topic { get; set; } = ContactTopic.General;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "I agree to be contacted about CGI services")]
    public bool ConsentToContact { get; set; }
}