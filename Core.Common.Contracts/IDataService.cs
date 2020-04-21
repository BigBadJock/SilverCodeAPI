using REST_Parser.Models;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IDataService<T> where T : class, IModel, new()
    {
        Task<T> Add(T model);
        Task<T> Update(T model);
        Task<bool> Delete(T model);
        Task<T> Get(int id);
        RestResult<T> Search(string restQuery);
        
    }
}
