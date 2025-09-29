using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasExpo.Models.Enums;

namespace EasExpo.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }

        public Booking Booking { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string Provider { get; set; }

        [MaxLength(100)]
        public string TransactionReference { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
