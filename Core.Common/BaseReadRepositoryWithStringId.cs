using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseReadRepositoryWithStringId<DBC, T> : BaseReadRepository<DBC, T>, IReadRepositoryWithStringId<DBC, T>
        where T : class, IModel, IModelWithStringId, new()
        where DBC : DbContext
    {

        public BaseReadRepositoryWithStringId(IDbContextFactory<DBC> dbcFactory, IRestToLinqParser<T> parser, ILogger<IReadRepositoryWithStringId<DBC, T>> logger) : base(dbcFactory, parser, logger)
        {

        }

        public virtual async Task<T> GetById(string id)
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
