using Core.Common.DataModels.Interfaces;
using System;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataServiceWithGuidId<T> : IDataService<T> where T : class, IModel, IModelWithGuidId, new()
    {
        Task<T> GetById(Guid id);
    }
}
