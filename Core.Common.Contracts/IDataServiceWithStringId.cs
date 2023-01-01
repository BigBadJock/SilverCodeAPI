using Core.Common.DataModels.Interfaces;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataServiceWithStringId<T> : IDataService<T> where T : class, IModel, IModelWithStringId, new()
    {
        Task<T> GetById(string id);
    }
}
