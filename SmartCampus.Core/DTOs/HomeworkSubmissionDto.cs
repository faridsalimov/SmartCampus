namespace SmartCampus.Core.DTOs
{
    public class HomeworkSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid HomeworkId { get; set; }
        public string HomeworkTitle { get; set; } = string.Empty;
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? SubmissionText { get; set; }
        public string? FileUrl { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string? Status { get; set; }
        public bool IsLate { get; set; }
        public GradeDto? Grade { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}