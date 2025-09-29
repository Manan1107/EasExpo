using System.ComponentModel.DataAnnotations;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerStallFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; }

        [MaxLength(100)]
        public string Size { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Rent per day (₹)")]
        public decimal RentPerDay { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Display(Name = "Availability status")]
        public StallStatus Status { get; set; } = StallStatus.Available;
    }
}
