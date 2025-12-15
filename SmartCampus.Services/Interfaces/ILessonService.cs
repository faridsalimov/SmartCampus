using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonDto>> GetAllLessonsAsync();
        Task<LessonDto?> GetLessonByIdAsync(Guid id);
        Task<IEnumerable<LessonDto>> GetLessonsByCourseAsync(Guid courseId);
        Task<IEnumerable<LessonDto>> GetLessonsByTeacherAsync(Guid teacherId);
        Task<IEnumerable<LessonDto>> GetUpcomingLessonsAsync(int days = 7);
        Task<IEnumerable<LessonDto>> GetLessonsByGroupAsync(Guid groupId);
        Task<IEnumerable<LessonDto>> GetTodayLessonsForTeacherAsync(Guid teacherId);
        Task<LessonDto?> GetLessonByTeacherWithOwnershipCheckAsync(Guid lessonId, Guid teacherId);
        Task<LessonDto> CreateLessonAsync(LessonDto lessonDto);
        Task UpdateLessonAsync(LessonDto lessonDto);
        Task DeleteLessonAsync(Guid id);
        Task CompleteLessonAsync(Guid id);
    }
}