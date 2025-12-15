namespace SmartCampus.Data.Repositories.Interfaces
{




    public interface IUnitOfWork : IAsyncDisposable
    {
        IStudentRepository StudentRepository { get; }
        ITeacherRepository TeacherRepository { get; }
        IGroupRepository GroupRepository { get; }
        ICourseRepository CourseRepository { get; }
        ILessonRepository LessonRepository { get; }
        IScheduleRepository ScheduleRepository { get; }
        IHomeworkRepository HomeworkRepository { get; }
        IHomeworkSubmissionRepository HomeworkSubmissionRepository { get; }
        IGradeRepository GradeRepository { get; }
        IAttendanceRepository AttendanceRepository { get; }
        IAnnouncementRepository AnnouncementRepository { get; }
        IMessageRepository MessageRepository { get; }





        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);




        Task BeginTransactionAsync(CancellationToken cancellationToken = default);




        Task CommitTransactionAsync(CancellationToken cancellationToken = default);




        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}