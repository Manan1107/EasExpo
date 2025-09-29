using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class FeedbackCreateViewModel
    {
        public int BookingId { get; set; }
        public string StallName { get; set; }

        [Range(1, 5)]
        [Display(Name = "Rating (1-5)")]
        public int Rating { get; set; }

        [MaxLength(500)]
        [Display(Name = "Comments")]
        public string Comments { get; set; }
    }
}
