using Ardalis.GuardClauses;
using Core.Common.Contracts;
using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using REST_Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseRepository<T> : BaseReadRepository<T>, IRepository<T>, IReadRepository<T> where T : class, IModel, new()
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataContext"></param>
        protected BaseRepository(DbContext dataContext, IRestToLinqParser<T> parser, ILogger<IRepository<T>> logger) : base(dataContext, parser, logger)
        {
        }
        public virtual async Task<T> Add(T entity, bool commit = true)
        {
            try
            {
                Guard.Against.Null(entity, nameof(entity));
                entity.LastUpdated = DateTime.Now;
                entity.Created = DateTime.Now;
                var added = dbset.Add(entity);
                if (commit)
                {
                    _ = await dataContext.SaveChangesAsync();
                }
                this.logger.LogInformation($"Repository: {this.GetType().Name} added new entity");
                return added.Entity;
            }
            catch (ArgumentNullException)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} tried to add a null entity");
                throw;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to add an entity", e);
                throw;
            }
        }

        public async Task AddBatch(IEnumerable<T> entities, int batchSize, IProgress<ProgressReport> progress)
        {
            string message = $"Saving {entities.Count()} {typeof(T).Name}";

            int total = entities.Count();
            int count = 0;
            int batchCount = 0;
            foreach (T entity in entities)
            {
                await this.Add(entity, false);
                if (batchCount > batchSize)
                {
                    await this.Commit();
                    batchCount = 0;
                }
                batchCount++;
                count++;
                progress.Report(new ProgressReport { Message = message, TotalProgress = total, CurrentProgress = count });
            }
            await this.Commit();
        }

        public async Task Commit()
        {
            _ = await dataContext.SaveChangesAsync().ConfigureAwait(false);

        }

        public virtual async Task<bool> Delete(T entity, bool commit = true)
        {
            try
            {
                Guard.Against.Null(entity, nameof(entity));
                dbset.Remove(entity);
                if (commit)
                {
                    await dataContext.SaveChangesAsync().ConfigureAwait(false);
                }
                this.logger.LogInformation($"Repository: {this.GetType().Name} deleted then entity: {JsonConvert.SerializeObject(entity)}");

                return true;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to delete an entity : {JsonConvert.SerializeObject(entity)}", e);
                return false;
            }

        }

        public virtual async Task<bool> Delete(Expression<Func<T, bool>> where, bool commit = true)
        {
            try
            {
                IEnumerable<T> objects = dbset.Where<T>(where).AsEnumerable();
                foreach (T obj in objects)
                {
                    dbset.Remove(obj);
                    this.logger.LogInformation($"Repository: {this.GetType().Name} deleting entity: {JsonConvert.SerializeObject(obj)}");
                }
                if (commit)
                {
                    await dataContext.SaveChangesAsync().ConfigureAwait(false);
                }
                this.logger.LogInformation($"Repository: {this.GetType().Name} deleting multiple entities successful");
                return true;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to deleting multiple entries", e);
                throw;
            }
        }

        public virtual async Task<T> Update(T entity, bool commit = true)
        {
            try
            {
                Guard.Against.Null(entity, nameof(entity));
                dbset.Attach(entity);
                dataContext.Entry(entity).State = EntityState.Modified;
                if (commit)
                {
                    await dataContext.SaveChangesAsync().ConfigureAwait(false);
                }
                this.logger.LogInformation($"Repository: {this.GetType().Name} updating entity: {JsonConvert.SerializeObject(entity)}");
                return entity;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to update entity: {JsonConvert.SerializeObject(entity)}", e);
                throw;
            }
        }
    }
}
