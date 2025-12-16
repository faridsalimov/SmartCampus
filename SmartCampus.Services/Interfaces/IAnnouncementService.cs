using SmartCampus.Core.DTOs;

namespace SmartCampus.Services.Interfaces
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync();
        Task<AnnouncementDto?> GetAnnouncementByIdAsync(Guid id);
        Task<AnnouncementDto?> GetAnnouncementByIdAsNoTrackingAsync(Guid id);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByTeacherAsync(Guid teacherId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByCourseAsync(Guid courseId);
        Task<IEnumerable<AnnouncementDto>> GetPublishedAnnouncementsAsync();
        Task<IEnumerable<AnnouncementDto>> GetRecentAnnouncementsAsync(int count = 10);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByGroupAsync(Guid groupId);
        Task<IEnumerable<AnnouncementDto>> GetAnnouncementsForStudentAsync(Guid studentId);
        Task<AnnouncementDto> CreateAnnouncementAsync(AnnouncementDto announcementDto);
        Task UpdateAnnouncementAsync(AnnouncementDto announcementDto);
        Task DeleteAnnouncementAsync(Guid id);
    }
}