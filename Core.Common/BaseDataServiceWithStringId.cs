using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseDataServiceWithStringId<T> : BaseDataService<T>, IDataServiceWithStringId<T> where T : class, IModel, IModelWithStringId, new()
    {

        public BaseDataServiceWithStringId(IRepositoryWithStringId<T> repository, ILogger<T> logger) : base((IRepository<T>)repository, (ILogger<T>)logger)
        {
            this.logger.LogInformation($"Creating DataService {this.GetType().Name}");
        }

        public virtual async Task<T> GetById(string id)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} getting entity by id");
                IRepositoryWithStringId<T> rep = (IRepositoryWithStringId<T>)this.repository;
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
    }
}
