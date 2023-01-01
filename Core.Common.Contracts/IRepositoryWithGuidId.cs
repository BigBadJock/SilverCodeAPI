using Core.Common.DataModels.Interfaces;

namespace Core.Common.Contracts
{
    public interface IRepositoryWithGuidId<T> : IRepository<T>, IReadRepositoryWithGuidId<T> where T : class, IModel, IModelWithGuidId, new()
    {
    }
}
