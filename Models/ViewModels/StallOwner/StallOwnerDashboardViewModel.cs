using System.Collections.Generic;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class StallOwnerDashboardViewModel
    {
        public int MyStallCount { get; set; }
        public int PendingBookings { get; set; }
        public int UpcomingBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public IReadOnlyList<OwnerStallSummaryViewModel> StallSummaries { get; set; } = new List<OwnerStallSummaryViewModel>();
        public IReadOnlyList<OwnerBookingDetailViewModel> UpcomingBookingDetails { get; set; } = new List<OwnerBookingDetailViewModel>();
        public IReadOnlyList<OwnerFeedbackViewModel> RecentFeedback { get; set; } = new List<OwnerFeedbackViewModel>();
    }
}
