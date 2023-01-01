using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;
using System;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseRepositoryWithGuidId<T> : BaseRepository<T>, IRepositoryWithGuidId<T> where T : class, IModel, IModelWithGuidId, new()
    {
        protected BaseRepositoryWithGuidId(DbContext dataContext, IRestToLinqParser<T> parser, ILogger<IRepository<T>> logger) : base(dataContext, parser, logger)
        {
        }

        public virtual async Task<T> GetById(Guid id)
        {
            this.logger.LogInformation($"Repository: {this.GetType().Name} getting entity for id {id}");
            T result = await this.dbset.FindAsync(id);
            if (result == null)
            {
                this.logger.LogInformation($"Repository: {this.GetType().Name} entity not found for id {id}");
            }
            else
            {
                this.logger.LogInformation($"Repository: {this.GetType().Name} entity found for id {id}");
            }
            return result;
        }
    }
}
