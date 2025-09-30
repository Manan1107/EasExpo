using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class FeedbackCreateViewModel
    {
        public int BookingId { get; set; }
        public string StallName { get; set; }

    [Required]
    [MaxLength(500)]
    [Display(Name = "Feedback")]
    public string Comments { get; set; }
    }
}
