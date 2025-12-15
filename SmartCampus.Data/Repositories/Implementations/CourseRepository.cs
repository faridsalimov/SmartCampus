using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class CourseRepository : Repository<Course>, ICourseRepository
    {
        public CourseRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Course>> GetAllAsync()
        {
            return await DbSet.Include(c => c.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(c => c.Group)
                              .ToListAsync();
        }

        public override async Task<Course?> GetByIdAsync(Guid id)
        {
            return await DbSet.Include(c => c.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(c => c.Group)
                              .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Course>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await DbSet.Where(c => c.TeacherId == teacherId)
                              .Include(c => c.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(c => c.Group)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetByGroupIdAsync(Guid groupId)
        {
            return await DbSet.Where(c => c.GroupId == groupId)
                              .Include(c => c.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(c => c.Group)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetActiveCoursesByGroupAsync(Guid groupId)
        {
            return await DbSet.Where(c => c.GroupId == groupId && c.IsActive)
                              .Include(c => c.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(c => c.Group)
                              .ToListAsync();
        }

        public async Task<Course?> GetByCourseCodeAsync(string code)
        {
            return await DbSet.Include(c => c.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(c => c.Group)
                              .FirstOrDefaultAsync(c => c.Code == code);
        }
    }
}