namespace SmartCampus.Core.DTOs
{
    public class ScheduleDto
    {
        public Guid Id { get; set; }
        public Guid? LessonId { get; set; }
        public string? LessonTitle { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public Guid? GroupId { get; set; }
        public string? GroupName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string? DayOfWeek { get; set; }
        public string? Location { get; set; }
        public string? Room { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}