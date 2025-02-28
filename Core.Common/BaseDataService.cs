﻿using Core.Common.Contracts;
using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
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

        public BaseDataService(IRepository<DBC, T> repository, ILogger<IDataService<DBC, T>> logger)
        {
            this.repository = repository;
            this.logger = logger;
            this.logger.LogInformation($"Creating DataService {this.GetType().Name}");

        }

        public virtual async Task<T> Add(T model)
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

        public virtual async Task<bool> Delete(Expression<Func<T, bool>> where)
        {
            try
            {
                this.logger.LogInformation($"DataService: {this.GetType().Name} deleting on condition: ${where}");
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

        public virtual ApiResult<T> Search(string restQuery)
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

        public virtual async Task<T> Update(T model)
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
