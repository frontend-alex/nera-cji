using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace  nera_cji.Models
{
    [Table("feedback")]
    public class Feedback
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("event_id")]
        public int EventId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("rating")]
        public int? Rating { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
