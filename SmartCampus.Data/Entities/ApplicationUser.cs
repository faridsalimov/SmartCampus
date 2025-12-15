using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace SmartCampus.Data.Entities
{



    public class ApplicationUser : IdentityUser
    {
        [StringLength(200)]
        public string? FullName { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(500)]
        public string? ProfilePhoto { get; set; }

        [StringLength(50)]
        public string? UserRole { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;


        public virtual Student? Student { get; set; }
        public virtual Teacher? Teacher { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
    }
}