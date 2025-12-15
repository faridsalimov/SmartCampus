using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class HomeworkRepository : Repository<Homework>, IHomeworkRepository
    {
        public HomeworkRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Homework>> GetAllAsync()
        {
            return await DbSet.Include(h => h.Group)
                              .Include(h => h.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderByDescending(h => h.DueDate)
                              .ToListAsync();
        }

        public override async Task<Homework?> GetByIdAsync(Guid id)
        {
            return await DbSet.Include(h => h.Group)
                              .Include(h => h.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<IEnumerable<Homework>> GetByGroupIdAsync(Guid groupId)
        {
            return await DbSet.Where(h => h.GroupId == groupId)
                              .Include(h => h.Group)
                              .Include(h => h.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderByDescending(h => h.DueDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Homework>> GetByCourseIdAsync(Guid courseId)
        {

            return await Task.FromResult(Enumerable.Empty<Homework>());
        }

        public async Task<IEnumerable<Homework>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await DbSet.Where(h => h.TeacherId == teacherId)
                              .Include(h => h.Group)
                              .Include(h => h.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderByDescending(h => h.DueDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Homework>> GetUpcomingHomeworkAsync(int days = 7)
        {
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(days);

            return await DbSet.Where(h => h.DueDate >= fromDate && h.DueDate <= toDate && h.IsActive)
                              .Include(h => h.Group)
                              .Include(h => h.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderBy(h => h.DueDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Homework>> GetOverdueHomeworkAsync()
        {
            return await DbSet.Where(h => h.DueDate < DateTime.UtcNow && h.IsActive)
                              .Include(h => h.Group)
                              .Include(h => h.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderBy(h => h.DueDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Homework>> GetActiveHomeworkAsync()
        {
            return await DbSet.Where(h => h.IsActive)
                              .Include(h => h.Group)
                              .Include(h => h.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderByDescending(h => h.CreatedDate)
                              .ToListAsync();
        }
    }
}