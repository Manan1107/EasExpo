using System;
using System.Collections.Generic;

namespace EasExpo.Models.ViewModels.Account
{
    public class AccountProfileViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    }
}
