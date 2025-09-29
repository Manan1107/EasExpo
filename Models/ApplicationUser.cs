using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EasExpo.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(128)]
        public string FullName { get; set; }

        [MaxLength(128)]
        public string CompanyName { get; set; }

        [MaxLength(256)]
        public string Address { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
