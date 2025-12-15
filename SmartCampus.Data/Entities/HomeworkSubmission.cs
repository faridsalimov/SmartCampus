using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCampus.Data.Entities
{



    public class HomeworkSubmission
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid HomeworkId { get; set; }

        [ForeignKey(nameof(HomeworkId))]
        public virtual Homework? Homework { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public virtual Student? Student { get; set; }

        [StringLength(2000)]
        public string? SubmissionText { get; set; }

        [StringLength(500)]
        public string? FileUrl { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Status { get; set; }

        public bool IsLate { get; set; } = false;

        public DateTime? UpdatedAt { get; set; }


        public virtual Grade? Grade { get; set; }
    }
}