using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IRepositoryWithGuidId<DBC, T> : IRepository<DBC, T>, IReadRepositoryWithGuidId<DBC, T>
        where T : class, IModel, IModelWithGuidId, new()
        where DBC : DbContext
    {
        Task<bool> Delete(Guid id, bool commit = true);

    }
}
