using System;
using System.Collections.Generic;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerStallDetailViewModel : OwnerStallSummaryViewModel
    {
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IReadOnlyList<OwnerBookingDetailViewModel> BookingHistory { get; set; } = new List<OwnerBookingDetailViewModel>();
        public IReadOnlyList<OwnerFeedbackViewModel> Feedback { get; set; } = new List<OwnerFeedbackViewModel>();
    }
}
