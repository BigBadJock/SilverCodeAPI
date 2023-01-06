using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IReadRepositoryWithIntId<DBC, T> : IReadRepository<DBC, T>
        where T : class, IModel, IModelWithIntId, new()
        where DBC : DbContext
    {
        Task<T> GetById(int id);
    }
}
