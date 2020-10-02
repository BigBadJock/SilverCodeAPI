using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.Extensions.Logging;
using REST_Parser.Models;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseDataService<T> : IDataService<T> where T : class, IModel, new()
    {
        protected IRepository<T> repository;
        protected ILogger<T> logger;

        public BaseDataService(IRepository<T> repository, ILogger<T> logger)
        {
            this.repository = repository;
            this.logger = logger;
            this.logger.LogInformation($"Creating DataService {this.GetType().Name}");

        }

        public async Task<T> Add(T model)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} adding new entity");
                return await this.repository.Add(model);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"DataService: {this.GetType().Name} error adding new entity: ${ex.Message}");
                throw;
            }
            finally
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} exiting add new entity");
            }

        }

        public async Task<bool> Delete(T model)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} deleting entity");
                return await this.repository.Delete(model);
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

        public async Task<bool> Delete(Expression<Func<T, bool>> where)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} deleting on condition: ${where.ToString()}");
                return await this.repository.Delete(where);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"DataService: {this.GetType().Name} error deleting on condition: ${ex.Message}");
                throw;
            }
            finally
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} exiting deleting on condition");
            }
        }

        public async Task<T> GetById(Guid id)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} getting by id: ${id.ToString()}");
                return await this.repository.GetById(id);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"DataService: {this.GetType().Name} error getting by id: ${ex.Message}");
                throw;
            }
            finally
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} exiting getting by id: ${id.ToString()}");
            }

        }

        public RestResult<T> Search(string restQuery)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} searching using restQuery ${restQuery}");
                return this.repository.GetAll(restQuery);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"DataService: {this.GetType().Name} error searching using restQuery: ${ex.Message}");
                throw;
            }
            finally
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} exiting searching using restQuery ${restQuery}");
            }
        }

        public async Task<T> Update(T model)
        {
           try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} updating entity");
                return await this.repository.Update(model);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"DataService: {this.GetType().Name} error updating entity: ${ex.Message}");
                throw;
            }
            finally
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} exiting updating entity");
            }
        }
    }
}
