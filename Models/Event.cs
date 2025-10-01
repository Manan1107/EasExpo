using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasExpo.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [MaxLength(100)]
        public string StallSize { get; set; }

    [Range(0, 1000000)]
    [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
    public decimal SlotPrice { get; set; }

        [Range(1, 1000)]
        public int TotalSlots { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public string OwnerId { get; set; }

        public ApplicationUser Owner { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Stall> Stalls { get; set; }
    }
}
