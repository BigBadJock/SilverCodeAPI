using Core.Common.Contracts;
using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseDataService<DBC, T> : IDataService<DBC, T>
        where T : class, IModel, new()
        where DBC : DbContext
    {
        protected IRepository<DBC, T> repository;
        protected ILogger<IDataService<DBC, T>> logger;

        protected BaseDataService(IRepository<DBC, T> repository, ILogger<IDataService<DBC, T>> logger)
        {
            this.repository = repository;
            this.logger = logger;
            this.logger.LogInformation($"Creating DataService {GetType().Name}");

        }

        public virtual async Task<T> Add(T model)
        {
            try
            {
                logger.LogInformation($"DataService: {GetType().Name} adding new entity");
                return await repository.Add(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataService: {Name} error adding new entity", GetType().Name);
                throw;
            }
            finally
            {
                logger.LogInformation($"DataService: {GetType().Name} exiting add new entity");
            }

        }

        public virtual async Task<bool> Delete(Expression<Func<T, bool>> where)
        {
            try
            {
                logger.LogInformation("DataService: {Name} deleting on condition", GetType().Name);
                return await repository.Delete(where);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataService: {Name} error deleting on condition", GetType().Name);
                throw;
            }
            finally
            {
                logger.LogInformation($"DataService: {GetType().Name} exiting deleting on condition");
            }
        }

        public IQueryable<T> GetAll()
        {
            try
            {
                logger.LogInformation($"DataService: {GetType().Name} retrieving all entities");
                return repository.GetAll();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataService: {Name} GetAll()", GetType().Name);
                throw;
            }
        }

        public virtual ApiResult<T> Search(string restQuery)
        {
            try
            {
                logger.LogInformation("DataService: {Name} searching using restQuery {Query}", GetType().Name, restQuery);
                return repository.GetAll(restQuery);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataService: {Name} error searching using restQuery", GetType().Name);
                throw;
            }
            finally
            {
                logger.LogInformation("DataService: {Name} exiting searching using restQuery", GetType().Name);
            }
        }

        public virtual async Task<T> Update(T model)
        {
            try
            {
                logger.LogInformation($"DataService: {GetType().Name} updating entity");
                return await repository.Update(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "DataService: {Name} error updating entity", GetType().Name);
                throw;
            }
            finally
            {
                logger.LogInformation($"DataService: {GetType().Name} exiting updating entity");
            }
        }
    }
}
