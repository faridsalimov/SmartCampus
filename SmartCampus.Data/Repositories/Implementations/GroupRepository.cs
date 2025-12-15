using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Group>> GetAllAsync()
        {
            return await DbSet.Include(g => g.Students)
                              .Include(g => g.Courses)
                              .OrderBy(g => g.Name)
                              .ToListAsync();
        }

        public async Task<Group?> GetByCodeAsync(string code)
        {
            return await DbSet.Include(g => g.Students)
                              .Include(g => g.Courses)
                              .FirstOrDefaultAsync(g => g.Code == code);
        }

        public async Task<IEnumerable<Group>> GetActiveGroupsAsync()
        {
            return await DbSet.Where(g => g.IsActive)
                              .Include(g => g.Students)
                              .Include(g => g.Courses)
                              .OrderBy(g => g.Name)
                              .ToListAsync();
        }

        public async Task<int> GetStudentCountByGroupAsync(Guid groupId)
        {
            return await DbSet.Where(g => g.Id == groupId)
                              .SelectMany(g => g.Students)
                              .CountAsync();
        }
    }
}