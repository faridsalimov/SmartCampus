using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetByGroupIdAsync(Guid groupId);
        Task<IEnumerable<Lesson>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<Lesson>> GetUpcomingLessonsAsync(int days = 7);
        Task<IEnumerable<Lesson>> GetCompletedLessonsAsync();


        Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId);
    }
}