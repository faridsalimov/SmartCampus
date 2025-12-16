namespace SmartCampus.Core.DTOs
{
    public class TeacherDto
    {
        public Guid Id { get; set; }
        public string TeacherId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ApplicationUserId { get; set; }
        public string? Department { get; set; }
        public string? Specialization { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}