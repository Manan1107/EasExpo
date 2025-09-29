using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EasExpo.Models.Enums;

namespace EasExpo.Models
{
    public class Stall
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; }

        [MaxLength(100)]
        public string Size { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000)]
        public decimal RentPerDay { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public StallStatus Status { get; set; } = StallStatus.Available;

        [Required]
        public string OwnerId { get; set; }

        public ApplicationUser Owner { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
