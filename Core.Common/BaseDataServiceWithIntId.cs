using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseDataServiceWithIntId<DBC, T> : BaseDataService<DBC, T>, IDataServiceWithIntId<DBC, T>
        where T : class, IModel, IModelWithIntId, new()
        where DBC : DbContext
    {
        private IRepositoryWithIntId<DBC, T> rep;

        public BaseDataServiceWithIntId(IRepositoryWithIntId<DBC, T> repository, ILogger<IDataServiceWithIntId<DBC, T>> logger) : base(repository, logger)
        {
            this.logger.LogInformation($"Creating DataService {this.GetType().Name}");
            rep = (IRepositoryWithIntId<DBC, T>)this.repository;

        }

        public virtual async Task<T> GetById(int id)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} getting entity by id");
                return await rep.GetById(id);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"DataService: {this.GetType().Name} error getting entity by id: ${ex.Message}");
                throw;
            }
            finally
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} exiting get entity by id");
            }
        }

        public virtual async Task<bool> Delete(int id, bool commit = true)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} deleting entity");
                return await rep.Delete(id, commit);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"DataService: {this.GetType().Name} error deleting entity: ${ex.Message}");
                throw;
            }
            finally
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} exiting delete entity");
            }
        }
    }
}
