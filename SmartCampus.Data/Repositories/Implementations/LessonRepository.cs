using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class LessonRepository : Repository<Lesson>, ILessonRepository
    {
        public LessonRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Lesson>> GetAllAsync()
        {
            return await DbSet.Include(l => l.Group)
                              .Include(l => l.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .ToListAsync();
        }

        public override async Task<Lesson?> GetByIdAsync(Guid id)
        {
            return await DbSet.Include(l => l.Group)
                              .Include(l => l.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Lesson>> GetByGroupIdAsync(Guid groupId)
        {
            return await DbSet.Where(l => l.GroupId == groupId)
                              .Include(l => l.Group)
                              .Include(l => l.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderBy(l => l.LessonNumber)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId)
        {

            return await Task.FromResult(Enumerable.Empty<Lesson>());
        }

        public async Task<IEnumerable<Lesson>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await DbSet.Where(l => l.TeacherId == teacherId)
                              .Include(l => l.Group)
                              .Include(l => l.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderBy(l => l.LessonDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetUpcomingLessonsAsync(int days = 7)
        {
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(days);

            return await DbSet.Where(l => l.LessonDate >= fromDate && l.LessonDate <= toDate && !l.IsCompleted)
                              .Include(l => l.Group)
                              .Include(l => l.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderBy(l => l.LessonDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Lesson>> GetCompletedLessonsAsync()
        {
            return await DbSet.Where(l => l.IsCompleted)
                              .Include(l => l.Group)
                              .Include(l => l.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .OrderByDescending(l => l.LessonDate)
                              .ToListAsync();
        }
    }
}