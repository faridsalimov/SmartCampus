using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface ITeacherRepository : IRepository<Teacher>
    {
        Task<Teacher?> GetByApplicationUserIdAsync(string applicationUserId);
        Task<Teacher?> GetByTeacherIdAsync(string teacherId);
        Task<IEnumerable<Teacher>> GetByDepartmentAsync(string department);
        Task<IEnumerable<Teacher>> GetActiveTeachersAsync();
    }
}