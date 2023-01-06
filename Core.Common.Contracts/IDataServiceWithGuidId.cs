using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataServiceWithGuidId<DBC, T> : IDataService<DBC, T>
        where T : class, IModel, IModelWithGuidId, new()
        where DBC : DbContext
    {
        Task<T> GetById(Guid id);
    }
}
