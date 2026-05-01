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
            this.logger.LogInformation("Repository: {Name} getting entity for id {Id}", this.GetType().Name, id);
            T result = await this.dbset.FindAsync(id);
            if (result == null)
            {
                this.logger.LogInformation("Repository: {Name} entity not found for id {Id}", this.GetType().Name, id);
            }
            else
            {
                this.logger.LogInformation("Repository: {Name} entity found for id {Id}", this.GetType().Name, id);
            }
            return result;
        }
    }
}
