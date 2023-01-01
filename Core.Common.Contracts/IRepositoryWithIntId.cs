using Core.Common.DataModels.Interfaces;

namespace Core.Common.Contracts
{
    public interface IRepositoryWithIntId<T> : IRepository<T>, IReadRepositoryWithIntId<T> where T : class, IModel, IModelWithIntId, new()
    {
    }
}
