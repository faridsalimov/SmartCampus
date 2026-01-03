using System.ComponentModel.DataAnnotations;

namespace SmartCampus.Core.DTOs
{
    public class StudentDto
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        public string? ApplicationUserId { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Group is required.")]
        public Guid GroupId { get; set; }

        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enrollment year is required.")]
        [Range(1900, 2100, ErrorMessage = "Invalid enrollment year.")]
        public int EnrollmentYear { get; set; }

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}