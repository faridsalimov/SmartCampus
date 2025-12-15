using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class TeacherRepository : Repository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Teacher>> GetAllAsync()
        {
            return await DbSet.Include(t => t.ApplicationUser)
                              .ToListAsync();
        }

        public override async Task<Teacher?> GetByIdAsync(Guid id)
        {
            return await DbSet.Include(t => t.ApplicationUser)
                              .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teacher?> GetByApplicationUserIdAsync(string applicationUserId)
        {
            return await DbSet.Include(t => t.ApplicationUser)
                              .FirstOrDefaultAsync(t => t.ApplicationUserId == applicationUserId);
        }

        public async Task<Teacher?> GetByTeacherIdAsync(string teacherId)
        {
            return await DbSet.Include(t => t.ApplicationUser)
                              .FirstOrDefaultAsync(t => t.TeacherId == teacherId);
        }

        public async Task<IEnumerable<Teacher>> GetByDepartmentAsync(string department)
        {
            return await DbSet.Where(t => t.Department == department && t.IsActive)
                              .Include(t => t.ApplicationUser)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Teacher>> GetActiveTeachersAsync()
        {
            return await DbSet.Where(t => t.IsActive)
                              .Include(t => t.ApplicationUser)
                              .ToListAsync();
        }
    }
}