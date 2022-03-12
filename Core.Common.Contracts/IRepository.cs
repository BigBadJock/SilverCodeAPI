using Core.Common.DataModels.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IRepository<T> : IReadRepository<T> where T : class, IModel, new() 
    {
        #region update
        Task<T> Add(T entity);
        Task<T> Update(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Delete(Expression<Func<T, bool>> where);
        #endregion
    }
}
