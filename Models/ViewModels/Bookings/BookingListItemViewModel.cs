using System;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Bookings
{
    public class BookingListItemViewModel
    {
        public int Id { get; set; }
        public string StallName { get; set; }
    public string EventName { get; set; }
    public int SlotNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal Amount { get; set; }
        public bool HasFeedback { get; set; }
    }
}
