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

        public async Task<IEnumerable<AttendanceDto>> CreateBulkAttendanceForLessonAsync(Guid lessonId)
        {
            try
            {
                var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
                if (lesson == null)
                    throw new InvalidOperationException($"Lesson with ID {lessonId} not found.");

                var students = await _unitOfWork.StudentRepository.GetByGroupIdAsync(lesson.GroupId);
                
                var attendanceRecords = new List<AttendanceRecord>();
                foreach (var student in students)
                {
                    var existingRecord = await _unitOfWork.AttendanceRepository.GetByStudentAndLessonAsync(student.Id, lessonId);
                    if (existingRecord == null)
                    {
                        var record = new AttendanceRecord
                        {
                            Id = Guid.NewGuid(),
                            StudentId = student.Id,
                            LessonId = lessonId,
                            Status = "Present",
                            AttendanceDate = lesson.LessonDate
                        };
                        attendanceRecords.Add(record);
                        await _unitOfWork.AttendanceRepository.AddAsync(record);
                    }
                }

                if (attendanceRecords.Count > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                return _mapper.Map<IEnumerable<AttendanceDto>>(attendanceRecords);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create bulk attendance: {ex.Message}", ex);
            }
        }

        public async Task<LessonSessionDto> StartLessonSessionAsync(Guid lessonId, Guid teacherId)
        {
            try
            {
                var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
                if (lesson == null)
                    throw new InvalidOperationException($"Lesson with ID {lessonId} not found.");

                if (lesson.TeacherId != teacherId)
                    throw new UnauthorizedAccessException("Only the assigned teacher can start this lesson.");

                lesson.IsActive = true;
                lesson.SessionStartTime = DateTime.UtcNow;
                _unitOfWork.LessonRepository.Update(lesson);

                var students = await _unitOfWork.StudentRepository.GetByGroupIdAsync(lesson.GroupId);

                var sessionDto = new LessonSessionDto
                {
                    LessonId = lesson.Id,
                    LessonTitle = lesson.Title,
                    GroupId = lesson.GroupId,
                    GroupName = lesson.Group?.Name ?? string.Empty,
                    TeacherId = lesson.TeacherId,
                    TeacherName = lesson.Teacher?.ApplicationUser?.FullName ?? string.Empty,
                    IsActive = true,
                    SessionStartTime = lesson.SessionStartTime,
                    Students = new List<StudentAttendanceSessionDto>()
                };

                foreach (var student in students)
                {
                    var existingRecord = await _unitOfWork.AttendanceRepository.GetByStudentAndLessonAsync(student.Id, lessonId);
                    
                    if (existingRecord == null)
                    {
                        existingRecord = new AttendanceRecord
                        {
                            Id = Guid.NewGuid(),
                            StudentId = student.Id,
                            LessonId = lessonId,
                            TeacherId = teacherId,
                            Status = "Present",
                            AttendanceDate = DateTime.UtcNow,
                            SessionStartTime = lesson.SessionStartTime
                        };
                        await _unitOfWork.AttendanceRepository.AddAsync(existingRecord);
                    }

                    sessionDto.Students.Add(new StudentAttendanceSessionDto
                    {
                        StudentId = student.Id,
                        StudentName = student.ApplicationUser?.FullName ?? string.Empty,
                        CurrentStatus = existingRecord.Status ?? "Present",
                        AttendanceRecordId = existingRecord.Id,
                        Remarks = existingRecord.Remarks
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return sessionDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to start lesson session: {ex.Message}", ex);
            }
        }

        public async Task EndLessonSessionAsync(Guid lessonId, Guid teacherId)
        {
            try
            {
                var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
                if (lesson == null)
                    throw new InvalidOperationException($"Lesson with ID {lessonId} not found.");

                if (lesson.TeacherId != teacherId)
                    throw new UnauthorizedAccessException("Only the assigned teacher can end this lesson.");

                lesson.IsActive = false;
                lesson.SessionEndTime = DateTime.UtcNow;
                lesson.IsCompleted = true;
                _unitOfWork.LessonRepository.Update(lesson);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to end lesson session: {ex.Message}", ex);
            }
        }

        public async Task<LessonSessionDto> GetActiveLessonSessionAsync(Guid lessonId, Guid teacherId)
        {
            try
            {
                var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
                if (lesson == null)
                    throw new InvalidOperationException($"Lesson with ID {lessonId} not found.");

                if (lesson.TeacherId != teacherId)
                    throw new UnauthorizedAccessException("You don't have permission to view this lesson session.");

                if (!lesson.IsActive)
                    throw new InvalidOperationException("This lesson session is not active.");

                var students = await _unitOfWork.StudentRepository.GetByGroupIdAsync(lesson.GroupId);
                var attendanceRecords = await _unitOfWork.AttendanceRepository.GetByLessonIdAsync(lessonId);

                var sessionDto = new LessonSessionDto
                {
                    LessonId = lesson.Id,
                    LessonTitle = lesson.Title,
                    GroupId = lesson.GroupId,
                    GroupName = lesson.Group?.Name ?? string.Empty,
                    TeacherId = lesson.TeacherId,
                    TeacherName = lesson.Teacher?.ApplicationUser?.FullName ?? string.Empty,
                    IsActive = lesson.IsActive,
                    SessionStartTime = lesson.SessionStartTime,
                    Students = new List<StudentAttendanceSessionDto>()
                };

                foreach (var student in students)
                {
                    var record = attendanceRecords.FirstOrDefault(ar => ar.StudentId == student.Id);
                    
                    sessionDto.Students.Add(new StudentAttendanceSessionDto
                    {
                        StudentId = student.Id,
                        StudentName = student.ApplicationUser?.FullName ?? string.Empty,
                        CurrentStatus = record?.Status ?? "Present",
                        AttendanceRecordId = record?.Id,
                        Remarks = record?.Remarks
                    });
                }

                return sessionDto;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get active lesson session: {ex.Message}", ex);
            }
        }

        public async Task UpdateSessionAttendanceAsync(Guid lessonId, Guid studentId, string status, Guid teacherId)
        {
            try
            {
                var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(lessonId);
                if (lesson == null)
                    throw new InvalidOperationException($"Lesson with ID {lessonId} not found.");

                if (lesson.TeacherId != teacherId)
                    throw new UnauthorizedAccessException("Only the assigned teacher can update attendance.");

                if (!lesson.IsActive)
                    throw new InvalidOperationException("The lesson session is not active.");

                var validStatuses = new[] { "Present", "Absent", "Late" };
                if (!validStatuses.Contains(status))
                    throw new InvalidOperationException($"Invalid status. Must be one of: {string.Join(", ", validStatuses)}");

                var record = await _unitOfWork.AttendanceRepository.GetByStudentAndLessonAsync(studentId, lessonId);
                
                if (record == null)
                {
                    record = new AttendanceRecord
                    {
                        Id = Guid.NewGuid(),
                        StudentId = studentId,
                        LessonId = lessonId,
                        TeacherId = teacherId,
                        Status = status,
                        AttendanceDate = DateTime.UtcNow,
                        SessionStartTime = lesson.SessionStartTime
                    };
                    await _unitOfWork.AttendanceRepository.AddAsync(record);
                }
                else
                {
                    record.Status = status;
                    record.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.AttendanceRepository.Update(record);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update attendance: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<AttendanceDto>> GetSessionAttendanceAsync(Guid lessonId)
        {
            try
            {
                var records = await _unitOfWork.AttendanceRepository.GetByLessonIdAsync(lessonId);
                return _mapper.Map<IEnumerable<AttendanceDto>>(records);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get session attendance: {ex.Message}", ex);
            }
        }
    }
}