using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Admin
{
    public class AdminUserFormViewModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "Is active")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.Password)]
        [Display(Name = "Temporary password")]
        public string Password { get; set; }

        public IEnumerable<string> AvailableRoles { get; set; }
    }
}
