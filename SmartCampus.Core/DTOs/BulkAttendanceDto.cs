namespace SmartCampus.Core.DTOs
{
    public class BulkAttendanceDto
    {
        public Guid LessonId { get; set; }
        public List<StudentAttendanceDto> StudentAttendances { get; set; } = new();
    }

    public class StudentAttendanceDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Status { get; set; } = "Present";
        public string? Remarks { get; set; }
    }
}
