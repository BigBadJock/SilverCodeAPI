using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IRepositoryWithIntId<DBC, T> : IRepository<DBC, T>, IReadRepositoryWithIntId<DBC, T>
        where T : class, IModel, IModelWithIntId, new()
        where DBC : DbContext
    {
        Task<bool> Delete(int id, bool commit = true);

    }
}
