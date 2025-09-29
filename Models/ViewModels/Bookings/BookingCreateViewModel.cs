using System;
using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class BookingCreateViewModel
    {
        public int StallId { get; set; }
        public string StallName { get; set; }
        public decimal RentPerDay { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }
    }
}
