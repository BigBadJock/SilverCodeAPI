using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Core.Common.Contracts
{
    public interface IRepositoryWithIntId<DBC, T> : IRepository<DBC, T>, IReadRepositoryWithIntId<DBC, T>
        where T : class, IModel, IModelWithIntId, new()
        where DBC : DbContext
    {
    }
}
