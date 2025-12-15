using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IGroupRepository : IRepository<Group>
    {
        Task<Group?> GetByCodeAsync(string code);
        Task<IEnumerable<Group>> GetActiveGroupsAsync();
        Task<int> GetStudentCountByGroupAsync(Guid groupId);
    }
}