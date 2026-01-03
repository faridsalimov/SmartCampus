using System.ComponentModel.DataAnnotations;

namespace SmartCampus.Core.DTOs
{
    public class LessonDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Lesson title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Content cannot exceed 2000 characters.")]
        public string? Content { get; set; }

        [Required(ErrorMessage = "Group is required.")]
        public Guid GroupId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Teacher is required.")]
        public Guid TeacherId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lesson number is required.")]
        [Range(1, 1000, ErrorMessage = "Lesson number must be between 1 and 1000.")]
        public int LessonNumber { get; set; }

        [Required(ErrorMessage = "Lesson date is required.")]
        public DateTime LessonDate { get; set; }

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string? Location { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsActive { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}