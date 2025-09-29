using System;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerFeedbackViewModel
    {
        public string StallName { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
