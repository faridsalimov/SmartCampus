using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AttendanceDto>> GetAllAttendanceAsync()
        {
            var records = await _unitOfWork.AttendanceRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AttendanceDto>>(records);
        }

        public async Task<AttendanceDto?> GetAttendanceByIdAsync(Guid id)
        {
            var record = await _unitOfWork.AttendanceRepository.GetByIdAsync(id);
            return _mapper.Map<AttendanceDto>(record);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByStudentAsync(Guid studentId)
        {
            var records = await _unitOfWork.AttendanceRepository.GetByStudentIdAsync(studentId);
            return _mapper.Map<IEnumerable<AttendanceDto>>(records);
        }

        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByLessonAsync(Guid lessonId)
        {
            var records = await _unitOfWork.AttendanceRepository.GetByLessonIdAsync(lessonId);
            return _mapper.Map<IEnumerable<AttendanceDto>>(records);
        }

        public async Task<decimal> GetStudentAttendancePercentageAsync(Guid studentId)
        {
            return await _unitOfWork.AttendanceRepository.GetStudentAttendancePercentageAsync(studentId);
        }


        public async Task<IEnumerable<AttendanceDto>> GetAttendanceByGroupAsync(Guid groupId)
        {

            return await GetAllAttendanceAsync();
        }


        public async Task<int> GetPresentCountForStudentAsync(Guid studentId)
        {
            var attendance = await GetAttendanceByStudentAsync(studentId);
            return attendance.Count(a => a.Status == "Present");
        }


        public async Task<int> GetAbsentCountForStudentAsync(Guid studentId)
        {
            var attendance = await GetAttendanceByStudentAsync(studentId);
            return attendance.Count(a => a.Status == "Absent");
        }

        public async Task<AttendanceDto> RecordAttendanceAsync(Guid studentId, Guid lessonId, string status)
        {
            var existingRecord = await _unitOfWork.AttendanceRepository.GetByStudentAndLessonAsync(studentId, lessonId);

            AttendanceRecord record;
            if (existingRecord != null)
            {
                existingRecord.Status = status;
                existingRecord.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.AttendanceRepository.Update(existingRecord);
                record = existingRecord;
            }
            else
            {
                record = new AttendanceRecord
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    LessonId = lessonId,
                    Status = status,
                    AttendanceDate = DateTime.UtcNow
                };
                await _unitOfWork.AttendanceRepository.AddAsync(record);
            }

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<AttendanceDto>(record);
        }

        public async Task UpdateAttendanceAsync(AttendanceDto attendanceDto)
        {
            var record = _mapper.Map<AttendanceRecord>(attendanceDto);
            record.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.AttendanceRepository.Update(record);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAttendanceAsync(Guid id)
        {
            var record = await _unitOfWork.AttendanceRepository.GetByIdAsync(id);
            if (record != null)
            {
                _unitOfWork.AttendanceRepository.Delete(record);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}