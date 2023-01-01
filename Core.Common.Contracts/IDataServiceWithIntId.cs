using Core.Common.DataModels.Interfaces;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataServiceWithIntId<T> : IDataService<T> where T : class, IModel, IModelWithIntId, new()
    {
        Task<T> GetById(int id);
    }
}
