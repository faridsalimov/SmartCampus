using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface ICourseRepository : IRepository<Course>
    {
        Task<IEnumerable<Course>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<Course>> GetByGroupIdAsync(Guid groupId);
        Task<IEnumerable<Course>> GetActiveCoursesByGroupAsync(Guid groupId);
        Task<Course?> GetByCourseCodeAsync(string code);
    }
}