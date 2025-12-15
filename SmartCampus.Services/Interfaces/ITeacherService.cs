using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherDto>> GetAllTeachersAsync();
        Task<TeacherDto?> GetTeacherByIdAsync(Guid id);
        Task<TeacherDto?> GetTeacherByApplicationUserIdAsync(string applicationUserId);
        Task<IEnumerable<TeacherDto>> GetTeachersByDepartmentAsync(string department);
        Task<int> GetTotalStudentsForTeacherAsync(Guid teacherId);
        Task<TeacherDto> CreateTeacherAsync(TeacherDto teacherDto);
        Task UpdateTeacherAsync(TeacherDto teacherDto);
        Task DeleteTeacherAsync(Guid id);
    }
}