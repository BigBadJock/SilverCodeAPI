using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataService<DBC, T>
        where T : class, IModel, new()
        where DBC : DbContext
    {
        Task<T> Add(T model);
        Task<T> Update(T model);
        Task<bool> Delete(T model);

        Task<bool> Delete(Expression<Func<T, bool>> where);
        ApiResult<T> Search(string restQuery);

    }
}
