using SmartCampus.Data.Entities;

namespace SmartCampus.Data.Repositories.Interfaces
{



    public interface IAnnouncementRepository : IRepository<Announcement>
    {
        Task<IEnumerable<Announcement>> GetByTeacherIdAsync(Guid teacherId);
        Task<IEnumerable<Announcement>> GetByCourseIdAsync(Guid courseId);
        Task<IEnumerable<Announcement>> GetPublishedAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetPinnedAnnouncementsAsync();
        Task<IEnumerable<Announcement>> GetRecentAnnouncementsAsync(int count = 10);
    }
}