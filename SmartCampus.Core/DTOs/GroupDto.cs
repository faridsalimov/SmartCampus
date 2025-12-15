namespace SmartCampus.Core.DTOs
{
    public class GroupDto
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? GroupCode { get; set; }
        public int AcademicYear { get; set; }
        public bool IsActive { get; set; }
        public int StudentCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}