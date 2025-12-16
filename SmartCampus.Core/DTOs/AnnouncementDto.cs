namespace SmartCampus.Core.DTOs
{
    public class AnnouncementDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string? TeacherPhoto { get; set; }
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public bool IsPinned { get; set; }
        public bool IsPublished { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}