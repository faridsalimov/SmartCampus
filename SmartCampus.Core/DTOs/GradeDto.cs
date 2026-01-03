namespace SmartCampus.Core.DTOs
{
    public class GradeDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = "Unknown";
        public Guid LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public Guid? GroupId { get; set; }
        public decimal Score { get; set; }
        public string? LetterGrade { get; set; }
        public string? Feedback { get; set; }
        public DateTime GradedDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}