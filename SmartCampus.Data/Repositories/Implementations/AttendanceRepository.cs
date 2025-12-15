using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class AttendanceRepository : Repository<AttendanceRecord>, IAttendanceRepository
    {
        public AttendanceRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<AttendanceRecord>> GetAllAsync()
        {
            return await DbSet.Include(ar => ar.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(ar => ar.Lesson)
                              .OrderByDescending(ar => ar.AttendanceDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetByStudentIdAsync(Guid studentId)
        {
            return await DbSet.Where(ar => ar.StudentId == studentId)
                              .Include(ar => ar.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(ar => ar.Lesson)
                              .OrderByDescending(ar => ar.AttendanceDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<AttendanceRecord>> GetByLessonIdAsync(Guid lessonId)
        {
            return await DbSet.Where(ar => ar.LessonId == lessonId)
                              .Include(ar => ar.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(ar => ar.Lesson)
                              .OrderBy(ar => ar.AttendanceDate)
                              .ToListAsync();
        }

        public async Task<AttendanceRecord?> GetByStudentAndLessonAsync(Guid studentId, Guid lessonId)
        {
            return await DbSet.Include(ar => ar.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(ar => ar.Lesson)
                              .FirstOrDefaultAsync(ar => ar.StudentId == studentId && ar.LessonId == lessonId);
        }

        public async Task<decimal> GetStudentAttendancePercentageAsync(Guid studentId)
        {
            var records = await DbSet.Where(ar => ar.StudentId == studentId).ToListAsync();
            if (records.Count == 0) return 0;

            var presentCount = records.Count(ar => ar.Status == "Present");
            return (decimal)presentCount / records.Count * 100;
        }

        public async Task<IEnumerable<AttendanceRecord>> GetByStudentAndDateRangeAsync(Guid studentId, DateTime startDate, DateTime endDate)
        {
            return await DbSet.Where(ar => ar.StudentId == studentId &&
                                          ar.AttendanceDate >= startDate &&
                                          ar.AttendanceDate <= endDate)
                              .Include(ar => ar.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(ar => ar.Lesson)
                              .OrderBy(ar => ar.AttendanceDate)
                              .ToListAsync();
        }
    }
}