using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCampus.Data.Entities
{
    public class Lesson
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Content { get; set; }

        [Required]
        public Guid GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual Group? Group { get; set; }

        [Required]
        public Guid TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public virtual Teacher? Teacher { get; set; }

        public int LessonNumber { get; set; }

        public DateTime LessonDate { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        public bool IsCompleted { get; set; } = false;

        public bool IsActive { get; set; } = false;

        public DateTime? SessionStartTime { get; set; }

        public DateTime? SessionEndTime { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}