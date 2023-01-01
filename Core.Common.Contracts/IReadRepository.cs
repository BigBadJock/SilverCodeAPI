using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Core.Common.Contracts
{
    public interface IReadRepository<T> where T : class, IModel, new()
    {

        DbSet<T> DbSet { get; }

        bool AlwaysIncludeChildren { get; set; }

        IQueryable<T> GetAll();
        ApiResult<T> GetAll(string restQuery);
    }
}
