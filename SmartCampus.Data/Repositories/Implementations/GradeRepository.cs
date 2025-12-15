using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class GradeRepository : Repository<Grade>, IGradeRepository
    {
        public GradeRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Grade>> GetAllAsync()
        {
            return await DbSet.Include(g => g.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(g => g.Course)
                              .Include(g => g.HomeworkSubmission)
                              .OrderByDescending(g => g.GradedDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetByStudentIdAsync(Guid studentId)
        {
            return await DbSet.Where(g => g.StudentId == studentId)
                              .Include(g => g.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(g => g.Course)
                              .Include(g => g.HomeworkSubmission)
                              .OrderByDescending(g => g.GradedDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetByCourseIdAsync(Guid courseId)
        {
            return await DbSet.Where(g => g.CourseId == courseId)
                              .Include(g => g.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(g => g.Course)
                              .OrderByDescending(g => g.GradedDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Grade>> GetByGroupIdAsync(Guid groupId)
        {
            return await DbSet.Where(g => g.GroupId == groupId)
                              .Include(g => g.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(g => g.Group)
                              .Include(g => g.Course)
                              .OrderByDescending(g => g.GradedDate)
                              .ToListAsync();
        }

        public async Task<Grade?> GetByHomeworkSubmissionIdAsync(Guid submissionId)
        {
            return await DbSet.Include(g => g.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(g => g.HomeworkSubmission)
                              .FirstOrDefaultAsync(g => g.HomeworkSubmissionId == submissionId);
        }

        public async Task<decimal?> GetStudentAverageGradeAsync(Guid studentId)
        {
            var grades = await DbSet.Where(g => g.StudentId == studentId).ToListAsync();
            return grades.Any() ? grades.Average(g => g.Score) : null;
        }

        public async Task<decimal?> GetGroupAverageGradeAsync(Guid groupId)
        {
            var grades = await DbSet.Where(g => g.GroupId == groupId).ToListAsync();
            return grades.Any() ? grades.Average(g => g.Score) : null;
        }
    }
}