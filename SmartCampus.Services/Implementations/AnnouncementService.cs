using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AnnouncementService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAllAnnouncementsAsync()
        {
            var announcements = await _unitOfWork.AnnouncementRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task<AnnouncementDto?> GetAnnouncementByIdAsync(Guid id)
        {
            var announcement = await _unitOfWork.AnnouncementRepository.GetByIdAsync(id);
            return _mapper.Map<AnnouncementDto>(announcement);
        }

        public async Task<AnnouncementDto?> GetAnnouncementByIdAsNoTrackingAsync(Guid id)
        {
            var announcement = await _unitOfWork.AnnouncementRepository.GetByIdAsNoTrackingAsync(id);
            return _mapper.Map<AnnouncementDto>(announcement);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByTeacherAsync(Guid teacherId)
        {
            var announcements = await _unitOfWork.AnnouncementRepository.GetByTeacherIdAsync(teacherId);
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByCourseAsync(Guid courseId)
        {
            var announcements = await _unitOfWork.AnnouncementRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetPublishedAnnouncementsAsync()
        {
            var announcements = await _unitOfWork.AnnouncementRepository.GetPublishedAnnouncementsAsync();
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetRecentAnnouncementsAsync(int count = 10)
        {
            var announcements = await _unitOfWork.AnnouncementRepository.GetRecentAnnouncementsAsync(count);
            return _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsByGroupAsync(Guid groupId)
        {

            return await GetPublishedAnnouncementsAsync();
        }

        public async Task<IEnumerable<AnnouncementDto>> GetAnnouncementsForStudentAsync(Guid studentId)
        {

            return await GetPublishedAnnouncementsAsync();
        }

        public async Task<AnnouncementDto> CreateAnnouncementAsync(AnnouncementDto announcementDto)
        {
            var announcement = _mapper.Map<Announcement>(announcementDto);
            announcement.Id = Guid.NewGuid();
            announcement.PublishedDate = DateTime.UtcNow;

            await _unitOfWork.AnnouncementRepository.AddAsync(announcement);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AnnouncementDto>(announcement);
        }

        public async Task UpdateAnnouncementAsync(AnnouncementDto announcementDto)
        {
            var announcement = _mapper.Map<Announcement>(announcementDto);
            announcement.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.AnnouncementRepository.Update(announcement);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAnnouncementAsync(Guid id)
        {
            var announcement = await _unitOfWork.AnnouncementRepository.GetByIdAsync(id);
            if (announcement != null)
            {
                _unitOfWork.AnnouncementRepository.Delete(announcement);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}