using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCampus.Data.Entities
{



    public class Grade
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

        public Guid? GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public virtual Group? Group { get; set; }

        [Range(0, 100)]
        public decimal Score { get; set; }

        [StringLength(2)]
        public string? LetterGrade { get; set; }

        [StringLength(500)]
        public string? Feedback { get; set; }

        public DateTime GradedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        public virtual Teacher? GradedByTeacher { get; set; }
    }
}