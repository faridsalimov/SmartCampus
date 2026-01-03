using System.ComponentModel.DataAnnotations;

namespace SmartCampus.Core.DTOs
{
    public class TeacherDto
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

        [StringLength(200, ErrorMessage = "Department cannot exceed 200 characters.")]
        public string? Department { get; set; }

        [StringLength(500, ErrorMessage = "Specialization cannot exceed 500 characters.")]
        public string? Specialization { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}