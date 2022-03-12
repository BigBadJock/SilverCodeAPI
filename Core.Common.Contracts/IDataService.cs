using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataService<T> where T : class, IModel, new()
    {
        Task<T> Add(T model);
        Task<T> Update(T model);
        Task<bool> Delete(T model);

        Task<bool> Delete(Expression<Func<T, bool>> where);
        Task<T> GetById(Guid id);
        ApiResult<T> Search(string restQuery);
        
    }
}
