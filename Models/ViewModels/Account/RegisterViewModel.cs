using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Display(Name = "Company name (for stall owners)")]
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(64, MinimumLength = 8)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Registering as")]
        public string UserType { get; set; }

        [Display(Name = "Supporting document URL")]
        public string DocumentUrl { get; set; }

        [Display(Name = "Additional notes")]
        [StringLength(256)]
        public string AdditionalNotes { get; set; }
    }
}
