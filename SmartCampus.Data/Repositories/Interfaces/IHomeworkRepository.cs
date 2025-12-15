using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IHomeworkRepository : IRepository<Homework>
    {
        Task<IEnumerable<Homework>> GetByGroupIdAsync(Guid groupId);
        Task<IEnumerable<Homework>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<Homework>> GetUpcomingHomeworkAsync(int days = 7);
        Task<IEnumerable<Homework>> GetOverdueHomeworkAsync();
        Task<IEnumerable<Homework>> GetActiveHomeworkAsync();


        Task<IEnumerable<Homework>> GetByCourseIdAsync(Guid courseId);
    }
}