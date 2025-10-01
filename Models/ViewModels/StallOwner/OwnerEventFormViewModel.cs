using System;
using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models.ViewModels.StallOwner
{
    public class OwnerEventFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(150)]
        [Display(Name = "Event name")]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime EndDate { get; set; }

        [MaxLength(100)]
        [Display(Name = "Stall size")]
        public string StallSize { get; set; }

        [Range(0, 1000000)]
        [Display(Name = "Price per stall (₹)")]
        public decimal SlotPrice { get; set; }

        [Range(1, 500)]
        [Display(Name = "Number of stalls")]
        public int TotalSlots { get; set; }

        [MaxLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}
