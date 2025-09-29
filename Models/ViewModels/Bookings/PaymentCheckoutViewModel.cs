using System;
using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class PaymentCheckoutViewModel
    {
        public int BookingId { get; set; }
        public string StallName { get; set; }
        public decimal Amount { get; set; }
        public string PaymentProvider { get; set; } = "Razorpay";

        [Display(Name = "Reference (optional)")]
        public string TransactionReference { get; set; }
    }
}
