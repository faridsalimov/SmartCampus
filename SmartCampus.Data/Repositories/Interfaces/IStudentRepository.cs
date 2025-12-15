using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetByApplicationUserIdAsync(string applicationUserId);
        Task<Student?> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Student>> GetByGroupIdAsync(Guid groupId);
        Task<IEnumerable<Student>> GetActiveStudentsAsync();
    }
}