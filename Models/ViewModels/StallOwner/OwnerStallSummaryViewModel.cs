using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerStallSummaryViewModel
    {
        public int StallId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; }
    public int SlotNumber { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public decimal RentPerDay { get; set; }
        public StallStatus Status { get; set; }
        public int TotalBookings { get; set; }
        public int PendingRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public OwnerBookingDetailViewModel NextBooking { get; set; }
    }
}
