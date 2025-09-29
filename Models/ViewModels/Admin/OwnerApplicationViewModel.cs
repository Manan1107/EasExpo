using System;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Admin
{
    public class OwnerApplicationViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ApplicantName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string DocumentUrl { get; set; }
        public string AdditionalNotes { get; set; }
        public ApplicationStatus Status { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
