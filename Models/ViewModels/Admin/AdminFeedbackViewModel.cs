using System;

namespace EasExpo.Models.ViewModels.Admin
{
    public class AdminFeedbackViewModel
    {
        public string StallName { get; set; }
        public string CustomerName { get; set; }
        public int Rating { get; set; }
        public string Comments { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
