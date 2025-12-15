using Microsoft.EntityFrameworkCore.Storage;

using SmartCampus.Data.Context;
using SmartCampus.Data.Repositories.Implementations;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories
{




    public class UnitOfWork : IUnitOfWork
    {
        private readonly SmartCampusDbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public IStudentRepository StudentRepository { get; }
        public ITeacherRepository TeacherRepository { get; }
        public IGroupRepository GroupRepository { get; }
        public ICourseRepository CourseRepository { get; }
        public ILessonRepository LessonRepository { get; }
        public IScheduleRepository ScheduleRepository { get; }
        public IHomeworkRepository HomeworkRepository { get; }
        public IHomeworkSubmissionRepository HomeworkSubmissionRepository { get; }
        public IGradeRepository GradeRepository { get; }
        public IAttendanceRepository AttendanceRepository { get; }
        public IAnnouncementRepository AnnouncementRepository { get; }
        public IMessageRepository MessageRepository { get; }

        public UnitOfWork(SmartCampusDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

            StudentRepository = new StudentRepository(_dbContext);
            TeacherRepository = new TeacherRepository(_dbContext);
            GroupRepository = new GroupRepository(_dbContext);
            CourseRepository = new CourseRepository(_dbContext);
            LessonRepository = new LessonRepository(_dbContext);
            ScheduleRepository = new ScheduleRepository(_dbContext);
            HomeworkRepository = new HomeworkRepository(_dbContext);
            HomeworkSubmissionRepository = new HomeworkSubmissionRepository(_dbContext);
            GradeRepository = new GradeRepository(_dbContext);
            AttendanceRepository = new AttendanceRepository(_dbContext);
            AnnouncementRepository = new AnnouncementRepository(_dbContext);
            MessageRepository = new MessageRepository(_dbContext);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            await _dbContext.DisposeAsync();
        }
    }
}