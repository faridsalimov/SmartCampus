using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class AnnouncementRepository : Repository<Announcement>, IAnnouncementRepository
    {
        public AnnouncementRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Announcement>> GetAllAsync()
        {
            return await DbSet.Include(a => a.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(a => a.Course)
                              .OrderByDescending(a => a.PublishedDate)
                              .ToListAsync();
        }

        public override async Task<Announcement?> GetByIdAsync(Guid id)
        {
            return await DbSet.Include(a => a.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(a => a.Course)
                              .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Announcement>> GetByTeacherIdAsync(Guid teacherId)
        {
            return await DbSet.Where(a => a.TeacherId == teacherId)
                              .Include(a => a.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(a => a.Course)
                              .OrderByDescending(a => a.PublishedDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetByCourseIdAsync(Guid courseId)
        {
            return await DbSet.Where(a => a.CourseId == courseId)
                              .Include(a => a.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(a => a.Course)
                              .OrderByDescending(a => a.PublishedDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetPublishedAnnouncementsAsync()
        {
            return await DbSet.Where(a => a.IsPublished)
                              .Include(a => a.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(a => a.Course)
                              .OrderByDescending(a => a.PublishedDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetPinnedAnnouncementsAsync()
        {
            return await DbSet.Where(a => a.IsPinned && a.IsPublished)
                              .Include(a => a.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(a => a.Course)
                              .OrderByDescending(a => a.PublishedDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Announcement>> GetRecentAnnouncementsAsync(int count = 10)
        {
            return await DbSet.Where(a => a.IsPublished)
                              .Include(a => a.Teacher)
                              .ThenInclude(t => t!.ApplicationUser)
                              .Include(a => a.Course)
                              .OrderByDescending(a => a.PublishedDate)
                              .Take(count)
                              .ToListAsync();
        }
    }
}