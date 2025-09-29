using System;
using System.ComponentModel.DataAnnotations;
using EasExpo.Models.Enums;

namespace EasExpo.Models
{
    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int StallId { get; set; }

        public Stall Stall { get; set; }

        [Required]
        public string CustomerId { get; set; }

        public ApplicationUser Customer { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
