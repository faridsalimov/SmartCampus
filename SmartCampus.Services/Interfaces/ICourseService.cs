using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<CourseDto?> GetCourseByIdAsync(Guid id);
        Task<IEnumerable<CourseDto>> GetCoursesByTeacherAsync(Guid teacherId);
        Task<IEnumerable<CourseDto>> GetCoursesByGroupAsync(Guid groupId);
        Task<CourseDto?> GetCourseBycodeAsync(string code);
        Task<CourseDto> CreateCourseAsync(CourseDto courseDto);
        Task UpdateCourseAsync(CourseDto courseDto);
        Task DeleteCourseAsync(Guid id);
    }
}