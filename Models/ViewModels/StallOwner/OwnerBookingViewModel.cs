using System;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerBookingViewModel
    {
        public int Id { get; set; }
        public string StallName { get; set; }
        public string CustomerName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
