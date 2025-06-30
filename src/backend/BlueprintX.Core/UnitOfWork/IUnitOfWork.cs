using RedBook.Core.Repositories;

namespace RedBook.Core.UnitOfWork
{
    public interface IRepositoryFactory : IDisposable
    {

        /// <summary>
        /// Creates repository object for given entity using the DbContext of UnitOfWork
        /// </summary>
        IRepositoryBase<TEntity> GetRepository<TEntity>() where TEntity : class;

        /// <summary>
        /// Saves all changes until now in this unit of work.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Saves all changes until now in this unit of work.
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// Removes all entities from tracking
        /// </summary>
        void DetachAllEntities();

        /// <summary>
        /// Rollbacks to last commit
        /// </summary>
        Task RollbackAsync();
    }
}
