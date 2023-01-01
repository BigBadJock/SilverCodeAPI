using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IRepository<T> : IReadRepository<T> where T : class, IModel, new()
    {
        #region update
        Task<T> Add(T entity, bool commit = true);
        Task<T> Update(T entity, bool commit = true);
        Task<bool> Delete(T entity, bool commit = true);
        Task<bool> Delete(Expression<Func<T, bool>> where, bool commit = true);
        Task AddBatch(IEnumerable<T> entities, int batchSize, IProgress<ProgressReport> progress);
        Task Commit();
        #endregion
    }
}
