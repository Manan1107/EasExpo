using System;
using System.ComponentModel.DataAnnotations;
using EasExpo.Models.Enums;

namespace EasExpo.Models
{
    public class StallOwnerApplication
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        [MaxLength(256)]
        public string DocumentUrl { get; set; }

        [MaxLength(256)]
        public string AdditionalNotes { get; set; }

        public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }

        public string ReviewedBy { get; set; }
    }
}
