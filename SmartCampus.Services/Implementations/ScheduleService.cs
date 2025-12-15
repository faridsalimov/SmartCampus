using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ScheduleDto>> GetAllSchedulesAsync()
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<ScheduleDto?> GetScheduleByIdAsync(Guid id)
        {
            var schedule = await _unitOfWork.ScheduleRepository.GetByIdAsync(id);
            return _mapper.Map<ScheduleDto>(schedule);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByTeacherAsync(Guid teacherId)
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetByTeacherIdAsync(teacherId);
            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByDateAsync(DateTime date)
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetByDateAsync(date);
            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByCourseAsync(Guid courseId)
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetByCourseIdAsync(courseId);
            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<IEnumerable<ScheduleDto>> GetSchedulesByGroupAsync(Guid groupId)
        {
            var schedules = await _unitOfWork.ScheduleRepository.GetAllAsync();

            return _mapper.Map<IEnumerable<ScheduleDto>>(schedules);
        }

        public async Task<ScheduleDto> CreateScheduleAsync(ScheduleDto scheduleDto)
        {
            var schedule = _mapper.Map<Schedule>(scheduleDto);
            schedule.Id = Guid.NewGuid();
            schedule.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.ScheduleRepository.AddAsync(schedule);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ScheduleDto>(schedule);
        }

        public async Task UpdateScheduleAsync(ScheduleDto scheduleDto)
        {
            var schedule = _mapper.Map<Schedule>(scheduleDto);
            schedule.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ScheduleRepository.Update(schedule);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteScheduleAsync(Guid id)
        {
            var schedule = await _unitOfWork.ScheduleRepository.GetByIdAsync(id);
            if (schedule != null)
            {
                _unitOfWork.ScheduleRepository.Delete(schedule);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}