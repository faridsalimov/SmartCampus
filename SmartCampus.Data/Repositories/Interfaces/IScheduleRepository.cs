using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IScheduleRepository : IRepository<Schedule>
    {
        Task<IEnumerable<Schedule>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<Schedule>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Schedule>> GetByCourseIdAsync(Guid courseId);
        Task<IEnumerable<Schedule>> GetActiveSchedulesAsync();
    }
}