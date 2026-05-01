using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseRepositoryWithIntId<DBC, T> : BaseRepository<DBC, T>, IRepositoryWithIntId<DBC, T>
        where T : class, IModel, IModelWithIntId, new()
        where DBC : DbContext
    {
        protected BaseRepositoryWithIntId(IDbContextFactory<DBC> dbContextFactory, IRestToLinqParser<T> parser, ILogger<IRepository<DBC, T>> logger) : base(dbContextFactory, parser, logger)
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

        public virtual async Task<bool> Delete(int id, bool commit = true)
        {
            try
            {
                T entity = await dbset.FindAsync(id);
                if (entity == null)
                {
                    this.logger.LogWarning("Repository: {Name} entity of type {Type} not found for id {Id}", this.GetType().Name, typeof(T).Name, id);
                    return false;
                }

                dbset.Remove(entity);
                if (commit)
                {
                    await dataContext.SaveChangesAsync().ConfigureAwait(false);
                }
                this.logger.LogInformation("Repository: {Name} deleted entity of type {Type} with id {Id}", this.GetType().Name, typeof(T).Name, id);

                return true;
            }
            catch (DbUpdateException ex)
            {
                this.logger.LogError("Repository: {name} failed throwing error: {error} when trying to delete id : [{id}]", this.GetType().Name, ex.Message, id);
                return false;
            }

        }
    }
}
