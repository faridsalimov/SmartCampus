using System.ComponentModel.DataAnnotations;

namespace SmartCampus.Core.DTOs
{
    public class AttendanceDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Student is required.")]
        public Guid StudentId { get; set; }

        public string TeacherName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lesson is required.")]
        public Guid LessonId { get; set; }

        public string LessonTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Attendance status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        [RegularExpression("^(Present|Absent|Late)$", ErrorMessage = "Status must be Present, Absent, or Late.")]
        public string? Status { get; set; }

        public bool IsPresent { get; set; }

        [Required(ErrorMessage = "Attendance date is required.")]
        public DateTime AttendanceDate { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? Remarks { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}