using System;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Account
{
    public class OwnerApplicationStatusViewModel
    {
        public bool HasApplication { get; set; }
        public ApplicationStatus? Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string ReviewedBy { get; set; }
        public string DocumentUrl { get; set; }
        public string AdditionalNotes { get; set; }

        public bool IsPending => Status == ApplicationStatus.Pending;
        public bool IsApproved => Status == ApplicationStatus.Approved;
        public bool IsRejected => Status == ApplicationStatus.Rejected;
    }
}
