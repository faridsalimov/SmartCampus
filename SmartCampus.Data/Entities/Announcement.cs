using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCampus.Data.Entities
{



    public class Announcement
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Content { get; set; }

        [Required]
        public Guid TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher? Teacher { get; set; }

        public Guid? CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course? Course { get; set; }

        public bool IsPinned { get; set; } = false;
        public bool IsPublished { get; set; } = true;

        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}