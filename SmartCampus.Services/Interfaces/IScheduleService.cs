using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<IEnumerable<ScheduleDto>> GetAllSchedulesAsync();
        Task<ScheduleDto?> GetScheduleByIdAsync(Guid id);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByTeacherAsync(Guid teacherId);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByDateAsync(DateTime date);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByCourseAsync(Guid courseId);
        Task<IEnumerable<ScheduleDto>> GetSchedulesByGroupAsync(Guid groupId);
        Task<ScheduleDto> CreateScheduleAsync(ScheduleDto scheduleDto);
        Task UpdateScheduleAsync(ScheduleDto scheduleDto);
        Task DeleteScheduleAsync(Guid id);
    }
}