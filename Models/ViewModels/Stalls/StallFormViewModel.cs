using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EasExpo.Models;
using EasExpo.Models.Enums;

namespace EasExpo.Models.ViewModels.Stalls
{
    public class StallFormViewModel
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

        [Display(Name = "Owner")]
        public string OwnerId { get; set; }

        public StallStatus Status { get; set; } = StallStatus.Available;

        public IEnumerable<ApplicationUser> AvailableOwners { get; set; }
    }
}
