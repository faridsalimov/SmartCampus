namespace SmartCampus.Data.Repositories.Interfaces
{




    public interface IRepository<TEntity> where TEntity : class
    {

        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> FindAsync(Func<TEntity, bool> predicate);
        Task<TEntity?> FirstOrDefaultAsync(Func<TEntity, bool> predicate);


        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);


        void Update(TEntity entity);


        void Delete(TEntity entity);
        void DeleteRange(IEnumerable<TEntity> entities);


        Task<bool> ExistsAsync(Func<TEntity, bool> predicate);


        Task<int> CountAsync();
    }
}