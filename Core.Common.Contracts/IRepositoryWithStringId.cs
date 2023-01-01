using Core.Common.DataModels.Interfaces;

namespace Core.Common.Contracts
{
    public interface IRepositoryWithStringId<T> : IRepository<T>, IReadRepositoryWithStringId<T> where T : class, IModel, IModelWithStringId, new()
    {
    }
}
