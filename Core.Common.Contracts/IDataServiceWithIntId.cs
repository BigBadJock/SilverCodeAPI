using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataServiceWithIntId<DBC, T> : IDataService<DBC, T>
        where T : class, IModel, IModelWithIntId, new()
        where DBC : DbContext
    {
        Task<T> GetById(int id);
        Task<bool> Delete(int id, bool commit = true);
    }
}
