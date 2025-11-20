using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace nera_cji.Models
{
    [Table("event_participants")]
    public class EventParticipant
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

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        [Column("status")]
        [MaxLength(50)]
        public string? Status { get; set; }
    }
}
