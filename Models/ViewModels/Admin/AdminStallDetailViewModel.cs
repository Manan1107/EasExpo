using System;
using System.Collections.Generic;
using EasExpo.Models.Enums;
using EasExpo.Models.ViewModels.StallOwner;

namespace EasExpo.Models.ViewModels.Admin
{
    public class AdminStallDetailViewModel
    {
        public int StallId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Size { get; set; }
        public string Description { get; set; }
        public decimal RentPerDay { get; set; }
        public StallStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerPhone { get; set; }
        public string OwnerCompany { get; set; }

        public int TotalBookings { get; set; }
        public int PendingRequests { get; set; }
        public decimal TotalRevenue { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }

        public IReadOnlyList<AdminStallBookingDetailViewModel> BookingHistory { get; set; } = Array.Empty<AdminStallBookingDetailViewModel>();
        public IReadOnlyList<OwnerFeedbackViewModel> Feedback { get; set; } = Array.Empty<OwnerFeedbackViewModel>();
    }
}
