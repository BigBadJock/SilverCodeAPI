using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseReadRepositoryWithIntId<DBC, T> : BaseReadRepository<DBC, T>, IReadRepositoryWithIntId<DBC, T>
        where T : class, IModel, IModelWithIntId, new()
        where DBC : DbContext
    {

        public BaseReadRepositoryWithIntId(IDbContextFactory<DBC> dbcFactory, IRestToLinqParser<T> parser, ILogger<IReadRepositoryWithIntId<DBC, T>> logger) : base(dbcFactory, parser, logger)
        {

        }

        public virtual async Task<T> GetById(int id)
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
