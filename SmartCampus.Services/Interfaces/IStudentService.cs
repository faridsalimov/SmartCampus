using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
        Task<StudentDto?> GetStudentByIdAsync(Guid id);
        Task<StudentDto?> GetStudentByApplicationUserIdAsync(string applicationUserId);
        Task<IEnumerable<StudentDto>> GetStudentsByGroupAsync(Guid groupId);
        Task<StudentDto> CreateStudentAsync(StudentDto studentDto);
        Task UpdateStudentAsync(StudentDto studentDto);
        Task DeleteStudentAsync(Guid id);
    }
}