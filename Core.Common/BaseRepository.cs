using Ardalis.GuardClauses;
using Core.Common.Contracts;
using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseRepository<DBC, T> : BaseReadRepository<DBC, T>, IRepository<DBC, T>, IReadRepository<DBC, T>
        where T : class, IModel, new()
        where DBC : DbContext
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataContext"></param>
        protected BaseRepository(IDbContextFactory<DBC> dbContextFactory, IRestToLinqParser<T> parser, ILogger<IRepository<DBC, T>> logger) : base(dbContextFactory, parser, logger)
        {
        }
        public virtual async Task<T> Add(T entity, bool commit = true)
        {
            try
            {
                Guard.Against.Null(entity, nameof(entity));
                entity.LastUpdated = DateTime.UtcNow;
                entity.Created = DateTime.UtcNow;
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
            var entityList = entities.ToList();
            int total = entityList.Count;
            string message = $"Saving {total} {typeof(T).Name}";
            int count = 0;
            int batchCount = 0;
            foreach (T entity in entityList)
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
                this.logger.LogInformation("Repository: {Name} deleted entity of type {Type}", this.GetType().Name, typeof(T).Name);

                return true;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, "Repository: {Name} failed when trying to delete entity of type {Type}", this.GetType().Name, typeof(T).Name);
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
                    this.logger.LogInformation("Repository: {Name} removing entity of type {Type}", this.GetType().Name, typeof(T).Name);
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
                this.logger.LogInformation("Repository: {Name} updated entity of type {Type}", this.GetType().Name, typeof(T).Name);
                return entity;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, "Repository: {Name} failed when trying to update entity of type {Type}", this.GetType().Name, typeof(T).Name);
                throw;
            }
        }
    }
}
