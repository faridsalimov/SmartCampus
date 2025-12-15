namespace SmartCampus.Core.DTOs
{
    public class AttendanceDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string? Status { get; set; }
        public bool IsPresent { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string? Remarks { get; set; }
        public string? Notes { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}