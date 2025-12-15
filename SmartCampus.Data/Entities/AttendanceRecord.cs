using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCampus.Data.Entities
{



    public class AttendanceRecord
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student? Student { get; set; }

        [Required]
        public Guid LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public virtual Lesson? Lesson { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        public DateTime AttendanceDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}