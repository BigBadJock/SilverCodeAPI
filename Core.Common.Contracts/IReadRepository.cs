using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using REST_Parser.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common.Contracts
{
    public interface IReadRepository<T> where T : class, IModel, new()
    {

        DbSet<T>  DbSet{ get;  }

        bool AlwaysIncludeChildren { get; set; }

        #region get by id
        Task<T> GetById(Guid id, bool includeChildren = false);
        #endregion

        IQueryable<T> GetAll();
        ApiResult<T> GetAll(string restQuery);
    }
}
