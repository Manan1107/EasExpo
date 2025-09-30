using System;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class PaymentCheckoutViewModel
    {
        public int BookingId { get; set; }
        public string StallName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string RazorpayOrderId { get; set; }
        public string RazorpayKey { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerContact { get; set; }
        public string Notes { get; set; }

        public int AmountInPaise => (int)Math.Round(Amount * 100m, MidpointRounding.AwayFromZero);
    }
}
