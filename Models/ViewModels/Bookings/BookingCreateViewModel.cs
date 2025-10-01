using System;
using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class BookingCreateViewModel
    {
        public int StallId { get; set; }
    public int EventId { get; set; }
        public string StallName { get; set; }
        public decimal RentPerDay { get; set; }
        public string EventName { get; set; }
        public string EventLocation { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        public string StallSize { get; set; }
    }
}
