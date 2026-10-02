using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseDataServiceWithStringId<DBC, T> : BaseDataService<DBC, T>, IDataServiceWithStringId<DBC, T>
        where T : class, IModel, IModelWithStringId, new()
        where DBC : DbContext
    {
        private readonly IRepositoryWithStringId<DBC, T> rep;

        public BaseDataServiceWithStringId(IRepositoryWithStringId<DBC, T> repository, ILogger<IDataServiceWithStringId<DBC, T>> logger) : base(repository, logger)
        {
            rep = repository;
        }

        public virtual async Task<T?> GetById(string id)
        {
            try
            {
                this.logger.LogInformation("DataService: {Name} getting entity by id", this.GetType().Name);
                return await rep.GetById(id);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DataService: {Name} error getting entity by id", this.GetType().Name);
                throw;
            }
            finally
            {
                this.logger.LogInformation("DataService: {Name} exiting get entity by id", this.GetType().Name);
            }
        }

        public virtual async Task<bool> Delete(string id, bool commit = true)
        {
            try
            {
                this.logger.LogInformation("DataService: {Name} deleting entity", this.GetType().Name);
                return await rep.Delete(id, commit);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DataService: {Name} error deleting entity", this.GetType().Name);
                throw;
            }
            finally
            {
                this.logger.LogInformation("DataService: {Name} exiting delete entity", this.GetType().Name);
            }
        }
    }
}
