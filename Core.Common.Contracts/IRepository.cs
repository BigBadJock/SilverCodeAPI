using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IRepository<T> where T : class, IModel, new()
    {

        #region get by id
        Task<T> GetById(long id);
        #endregion

        IQueryable<T> GetAll();

        #region update
        Task<T> Add(T entity);
        Task<T> Update(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Delete(Expression<Func<T, bool>> where);
        #endregion
    }
}
