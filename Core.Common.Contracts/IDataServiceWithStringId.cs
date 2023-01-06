using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataServiceWithStringId<DBC, T> : IDataService<DBC, T>
        where T : class, IModel, IModelWithStringId, new()
        where DBC : DbContext
    {
        Task<T> GetById(string id);
    }
}
