using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IAttendanceRepository : IRepository<AttendanceRecord>
    {
        Task<IEnumerable<AttendanceRecord>> GetByStudentIdAsync(Guid studentId);
        Task<IEnumerable<AttendanceRecord>> GetByLessonIdAsync(Guid lessonId);
        Task<AttendanceRecord?> GetByStudentAndLessonAsync(Guid studentId, Guid lessonId);
        Task<decimal> GetStudentAttendancePercentageAsync(Guid studentId);
        Task<IEnumerable<AttendanceRecord>> GetByStudentAndDateRangeAsync(Guid studentId, DateTime startDate, DateTime endDate);
    }
}