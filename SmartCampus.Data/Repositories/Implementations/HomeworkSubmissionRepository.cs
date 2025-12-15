using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class HomeworkSubmissionRepository : Repository<HomeworkSubmission>, IHomeworkSubmissionRepository
    {
        public HomeworkSubmissionRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<HomeworkSubmission>> GetAllAsync()
        {
            return await DbSet.Include(hs => hs.Homework)
                              .Include(hs => hs.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(hs => hs.Grade)
                              .OrderByDescending(hs => hs.SubmissionDate)
                              .ToListAsync();
        }

        public override async Task<HomeworkSubmission?> GetByIdAsync(Guid id)
        {
            return await DbSet.Include(hs => hs.Homework)
                              .Include(hs => hs.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(hs => hs.Grade)
                              .FirstOrDefaultAsync(hs => hs.Id == id);
        }

        public async Task<IEnumerable<HomeworkSubmission>> GetByHomeworkIdAsync(Guid homeworkId)
        {
            return await DbSet.Where(hs => hs.HomeworkId == homeworkId)
                              .Include(hs => hs.Homework)
                              .Include(hs => hs.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(hs => hs.Grade)
                              .OrderByDescending(hs => hs.SubmissionDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<HomeworkSubmission>> GetByStudentIdAsync(Guid studentId)
        {
            return await DbSet.Where(hs => hs.StudentId == studentId)
                              .Include(hs => hs.Homework)
                              .Include(hs => hs.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(hs => hs.Grade)
                              .OrderByDescending(hs => hs.SubmissionDate)
                              .ToListAsync();
        }

        public async Task<HomeworkSubmission?> GetByStudentAndHomeworkAsync(Guid studentId, Guid homeworkId)
        {
            return await DbSet.Include(hs => hs.Homework)
                              .Include(hs => hs.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(hs => hs.Grade)
                              .FirstOrDefaultAsync(hs => hs.StudentId == studentId && hs.HomeworkId == homeworkId);
        }

        public async Task<IEnumerable<HomeworkSubmission>> GetUngradeSubmissionsAsync()
        {
            return await DbSet.Where(hs => hs.Grade == null)
                              .Include(hs => hs.Homework)
                              .Include(hs => hs.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(hs => hs.Grade)
                              .OrderBy(hs => hs.SubmissionDate)
                              .ToListAsync();
        }




        public async Task<IEnumerable<HomeworkSubmission>> GetSubmissionsByHomeworkAsync(Guid homeworkId)
        {
            return await GetByHomeworkIdAsync(homeworkId);
        }




        public async Task<IEnumerable<HomeworkSubmission>> GetSubmissionsByStudentAsync(Guid studentId)
        {
            return await GetByStudentIdAsync(studentId);
        }




        public async Task<HomeworkSubmission?> GetStudentSubmissionAsync(Guid homeworkId, Guid studentId)
        {
            return await GetByStudentAndHomeworkAsync(studentId, homeworkId);
        }




        public async Task<IEnumerable<HomeworkSubmission>> GetUngradedSubmissionsAsync(Guid homeworkId)
        {
            return await DbSet.Where(hs => hs.HomeworkId == homeworkId && hs.Grade == null)
                              .Include(hs => hs.Homework)
                              .Include(hs => hs.Student)
                              .ThenInclude(s => s!.ApplicationUser)
                              .Include(hs => hs.Grade)
                              .OrderBy(hs => hs.SubmissionDate)
                              .ToListAsync();
        }
    }
}