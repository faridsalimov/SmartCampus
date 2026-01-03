using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Student>> GetAllAsync()
        {
            return await DbSet.Include(s => s.ApplicationUser)
                              .Include(s => s.Group)
                              .ToListAsync();
        }

        public override async Task<Student?> GetByIdAsync(Guid id)
        {
            return await DbSet.Include(s => s.ApplicationUser)
                              .Include(s => s.Group)
                              .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student?> GetByApplicationUserIdAsync(string applicationUserId)
        {
            return await DbSet.Include(s => s.ApplicationUser)
                              .Include(s => s.Group)
                              .FirstOrDefaultAsync(s => s.ApplicationUserId == applicationUserId);
        }

        public async Task<IEnumerable<Student>> GetByGroupIdAsync(Guid groupId)
        {
            return await DbSet.Where(s => s.GroupId == groupId)
                              .Include(s => s.ApplicationUser)
                              .Include(s => s.Group)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Student>> GetActiveStudentsAsync()
        {
            return await DbSet.Where(s => s.IsActive)
                              .Include(s => s.ApplicationUser)
                              .Include(s => s.Group)
                              .ToListAsync();
        }
    }
}