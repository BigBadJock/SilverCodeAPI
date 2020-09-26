using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using REST_Parser.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IRepository<T> where T : class, IModel, new()
    {

        DbSet<T>  DbSet{ get;  }

        #region get by id
        Task<T> GetById(long id, bool includeChildren = false);
        #endregion

        IQueryable<T> GetAll(bool includeChildren = false);
        RestResult<T> GetAll(string restQuery, bool includeChildren = false);

        #region update
        Task<T> Add(T entity);
        Task<T> Update(T entity);
        Task<bool> Delete(T entity);
        Task<bool> Delete(Expression<Func<T, bool>> where);
        #endregion
    }
}
