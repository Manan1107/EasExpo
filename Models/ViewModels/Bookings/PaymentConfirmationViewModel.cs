using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class PaymentConfirmationViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public string RazorpayOrderId { get; set; }

        [Required]
        public string RazorpayPaymentId { get; set; }

        [Required]
        public string RazorpaySignature { get; set; }
    }
}
