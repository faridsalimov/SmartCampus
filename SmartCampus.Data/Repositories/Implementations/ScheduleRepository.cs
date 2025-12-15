using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class ScheduleRepository : Repository<Schedule>, IScheduleRepository
    {
        public ScheduleRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Schedule>> GetAllAsync()
        {
            return await DbSet.Include(s => s.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(s => s.Lesson)
                              .Include(s => s.Course)
                              .OrderBy(s => s.StartTime)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await DbSet.Where(s => s.TeacherId == teacherId && s.IsActive)
                              .Include(s => s.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(s => s.Lesson)
                              .Include(s => s.Course)
                              .OrderBy(s => s.StartTime)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await DbSet.Where(s => s.StartTime >= startDate && s.StartTime < endDate && s.IsActive)
                              .Include(s => s.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(s => s.Lesson)
                              .Include(s => s.Course)
                              .OrderBy(s => s.StartTime)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetByCourseIdAsync(Guid courseId)
        {
            return await DbSet.Where(s => s.CourseId == courseId && s.IsActive)
                              .Include(s => s.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(s => s.Lesson)
                              .Include(s => s.Course)
                              .OrderBy(s => s.StartTime)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Schedule>> GetActiveSchedulesAsync()
        {
            return await DbSet.Where(s => s.IsActive)
                              .Include(s => s.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(s => s.Lesson)
                              .Include(s => s.Course)
                              .OrderBy(s => s.StartTime)
                              .ToListAsync();
        }
    }
}