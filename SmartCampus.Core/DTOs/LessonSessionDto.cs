namespace SmartCampus.Core.DTOs
{
    public class LessonSessionDto
    {
        public Guid LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public Guid GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }
        public List<StudentAttendanceSessionDto> Students { get; set; } = new();
    }

    public class StudentAttendanceSessionDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentIdNumber { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = "Present";
        public Guid? AttendanceRecordId { get; set; }
        public string? Remarks { get; set; }
    }
}
