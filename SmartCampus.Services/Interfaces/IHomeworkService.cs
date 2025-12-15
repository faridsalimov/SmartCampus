using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IHomeworkService
    {
        Task<IEnumerable<HomeworkDto>> GetAllHomeworkAsync();
        Task<HomeworkDto?> GetHomeworkByIdAsync(Guid id);
        Task<IEnumerable<HomeworkDto>> GetHomeworkByCourseAsync(Guid courseId);
        Task<IEnumerable<HomeworkDto>> GetHomeworkByTeacherAsync(Guid teacherId);
        Task<IEnumerable<HomeworkDto>> GetUpcomingHomeworkAsync(int days = 7);


        Task<IEnumerable<HomeworkDto>> GetHomeworkByGroupAsync(Guid groupId);
        Task<IEnumerable<HomeworkDto>> GetHomeworkByStudentAsync(Guid studentId);
        Task<IEnumerable<HomeworkDto>> GetPendingHomeworkForTeacherAsync(Guid teacherId);


        Task MarkHomeworkCompletedByStudentAsync(Guid homeworkId, Guid studentId);


        Task<HomeworkDto?> GetHomeworkByTeacherWithOwnershipCheckAsync(Guid homeworkId, Guid teacherId);

        Task<HomeworkDto> CreateHomeworkAsync(HomeworkDto homeworkDto);
        Task UpdateHomeworkAsync(HomeworkDto homeworkDto);
        Task DeleteHomeworkAsync(Guid id);
    }
}