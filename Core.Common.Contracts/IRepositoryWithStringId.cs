using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Common.Contracts
{
    public interface IRepositoryWithStringId<DBC, T> : IRepository<DBC, T>, IReadRepositoryWithStringId<DBC, T>
        where T : class, IModel, IModelWithStringId, new()
        where DBC : DbContext
    {
    }
}
