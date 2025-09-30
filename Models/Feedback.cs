using System;
using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        public Booking Booking { get; set; }

    [Range(1, 5)]
    public int? Rating { get; set; }

        [MaxLength(500)]
        public string Comments { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
