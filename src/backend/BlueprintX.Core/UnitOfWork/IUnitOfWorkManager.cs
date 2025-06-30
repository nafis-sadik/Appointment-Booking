using Microsoft.EntityFrameworkCore.Storage;

namespace RedBook.Core.UnitOfWork
{
    public interface IUnitOfWorkManager
    {
        /// <summary>
        /// Returns Repository Factory.
        /// </summary>
        public IRepositoryFactory GetRepositoryFactory();

        /// <summary>
        /// Starts a unit of work.
        /// </summary>
        public IDbContextTransaction BeginTransaction();

        /// <summary>
        /// Commits operations to database asynchronously.
        /// </summary>
        public Task CommitAsync();

        /// <summary>
        /// Rollbacks operations to last commit asynchronously.
        /// </summary>
        public Task RollbackAsync();
    }
}
