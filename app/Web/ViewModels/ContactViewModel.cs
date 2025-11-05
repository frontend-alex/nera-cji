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
    [Display(Name = "Inquiry type")]
    public ContactTopic? Topic { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Your email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone")]
    [StringLength(30)]
    public string? Phone { get; set; }

    [Required]
    [StringLength(60)]
    [Display(Name = "Your first name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    [Display(Name = "Your last name")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Company")]
    public string Organization { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Job title")]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Display(Name = "Country")]
    public string Country { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    [Display(Name = "Message")]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "I agree to be contacted about CGI services")]
    public bool ConsentToContact { get; set; }
}
