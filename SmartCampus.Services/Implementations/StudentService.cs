using AutoMapper;

using SmartCampus.Core.DTOs;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;
using SmartCampus.Services.Interfaces;

namespace SmartCampus.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.StudentRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<StudentDto>>(students);
        }

        public async Task<StudentDto?> GetStudentByIdAsync(Guid id)
        {
            var student = await _unitOfWork.StudentRepository.GetByIdAsync(id);
            return _mapper.Map<StudentDto>(student);
        }

        public async Task<StudentDto?> GetStudentByApplicationUserIdAsync(string applicationUserId)
        {
            var student = await _unitOfWork.StudentRepository.GetByApplicationUserIdAsync(applicationUserId);
            return _mapper.Map<StudentDto>(student);
        }

        public async Task<IEnumerable<StudentDto>> GetStudentsByGroupAsync(Guid groupId)
        {
            var students = await _unitOfWork.StudentRepository.GetByGroupIdAsync(groupId);
            return _mapper.Map<IEnumerable<StudentDto>>(students);
        }

        public async Task<StudentDto> CreateStudentAsync(StudentDto studentDto)
        {
            if (studentDto == null)
                throw new ArgumentNullException(nameof(studentDto));

            if (studentDto.GroupId == Guid.Empty)
                throw new InvalidOperationException("Group is required.");

            if (string.IsNullOrWhiteSpace(studentDto.ApplicationUserId))
                throw new InvalidOperationException("ApplicationUserId is required.");

            try
            {
                var existingAppUser = await _unitOfWork.StudentRepository.GetByApplicationUserIdAsync(studentDto.ApplicationUserId);
                if (existingAppUser != null)
                    throw new InvalidOperationException("This user is already linked to another student record.");

                var student = _mapper.Map<Student>(studentDto);
                student.Id = Guid.NewGuid();
                student.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.StudentRepository.AddAsync(student);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<StudentDto>(student);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create student: {ex.Message}", ex);
            }
        }

        public async Task UpdateStudentAsync(StudentDto studentDto)
        {
            if (studentDto == null)
                throw new ArgumentNullException(nameof(studentDto));

            if (studentDto.Id == Guid.Empty)
                throw new InvalidOperationException("Student Id is required for update.");

            if (studentDto.GroupId == Guid.Empty)
                throw new InvalidOperationException("Group is required.");

            try
            {
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentDto.Id);
                if (student == null)
                    throw new InvalidOperationException($"Student with Id {studentDto.Id} not found.");

                student = _mapper.Map(studentDto, student);
                student.UpdatedAt = DateTime.UtcNow;

                _unitOfWork.StudentRepository.Update(student);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update student: {ex.Message}", ex);
            }
        }

        public async Task DeleteStudentAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Student Id is required.");

            try
            {
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(id);
                if (student == null)
                    throw new InvalidOperationException($"Student with Id {id} not found.");

                _unitOfWork.StudentRepository.Delete(student);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete student: {ex.Message}", ex);
            }
        }
    }
}