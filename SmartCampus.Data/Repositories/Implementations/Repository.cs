using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{




    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly SmartCampusDbContext DbContext;
        protected readonly DbSet<TEntity> DbSet;

        protected Repository(SmartCampusDbContext dbContext)
        {
            DbContext = dbContext;
            DbSet = dbContext.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(Guid id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<TEntity?> GetByIdAsNoTrackingAsync(Guid id)
        {
            return await DbSet.AsNoTracking().FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public virtual async Task<IEnumerable<TEntity>> FindAsync(Func<TEntity, bool> predicate)
        {


            return await Task.FromResult(DbSet.Where(predicate).ToList());
        }

        public virtual async Task<TEntity?> FirstOrDefaultAsync(Func<TEntity, bool> predicate)
        {


            return await Task.FromResult(DbSet.FirstOrDefault(predicate));
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await DbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            await DbSet.AddRangeAsync(entities);
        }

        public virtual void Update(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DbSet.Update(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DbSet.Remove(entity);
        }

        public virtual void DeleteRange(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            DbSet.RemoveRange(entities);
        }

        public virtual async Task<bool> ExistsAsync(Func<TEntity, bool> predicate)
        {
            return await Task.FromResult(DbSet.Any(predicate));
        }

        public virtual async Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }
    }
}