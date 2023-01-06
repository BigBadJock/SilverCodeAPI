using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IRepository<DBC, T> : IReadRepository<DBC, T>
        where T : class, IModel, new()
        where DBC : DbContext
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
