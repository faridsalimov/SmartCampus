using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCampus.Data.Entities
{



    public class Schedule
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public virtual Lesson? Lesson { get; set; }

        [Required]
        public Guid TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher? Teacher { get; set; }

        public Guid? CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public virtual Course? Course { get; set; }

        public Guid? GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual Group? Group { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [StringLength(50)]
        public string? DayOfWeek { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}