using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Web.Utilities
{




    public class UserContextHelper
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserContextHelper(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }




        public async Task<Guid?> GetStudentGroupIdAsync(string userId)
        {
            try
            {
                var student = await _unitOfWork.StudentRepository.GetByApplicationUserIdAsync(userId);
                return student?.GroupId;
            }
            catch
            {
                return null;
            }
        }




        public async Task<IEnumerable<Guid>> GetTeacherGroupIdsAsync(string userId)
        {
            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByApplicationUserIdAsync(userId);
                if (teacher == null)
                    return Enumerable.Empty<Guid>();


                var lessons = await _unitOfWork.LessonRepository.GetByTeacherIdAsync(teacher.Id);


                var groupIds = lessons
                    .Select(l => l.GroupId)
                    .Distinct()
                    .ToList();

                return groupIds;
            }
            catch
            {
                return Enumerable.Empty<Guid>();
            }
        }




        public async Task<bool> IsStudentInGroupAsync(Guid studentId, Guid groupId)
        {
            try
            {
                var student = await _unitOfWork.StudentRepository.GetByIdAsync(studentId);
                return student?.GroupId == groupId;
            }
            catch
            {
                return false;
            }
        }




        public async Task<bool> IsTeacherTeachingGroupAsync(Guid teacherId, Guid groupId)
        {
            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                    return false;

                var lessons = await _unitOfWork.LessonRepository.GetByTeacherIdAsync(teacher.Id);
                return lessons.Any(l => l.GroupId == groupId);
            }
            catch
            {
                return false;
            }
        }




        public async Task<Guid?> GetStudentIdByUserIdAsync(string userId)
        {
            try
            {
                var student = await _unitOfWork.StudentRepository.GetByApplicationUserIdAsync(userId);
                return student?.Id;
            }
            catch
            {
                return null;
            }
        }




        public async Task<Guid?> GetTeacherIdByUserIdAsync(string userId)
        {
            try
            {
                var teacher = await _unitOfWork.TeacherRepository.GetByApplicationUserIdAsync(userId);
                return teacher?.Id;
            }
            catch
            {
                return null;
            }
        }
    }
}