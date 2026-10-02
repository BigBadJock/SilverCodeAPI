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
        /// <param name="dbContextFactory">factory used to create the DbContext owned by this repository</param>
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
                this.logger.LogInformation("Repository: {Name} added new entity of type {Type}", this.GetType().Name, typeof(T).Name);
                return added.Entity;
            }
            catch (ArgumentNullException)
            {
                this.logger.LogError("Repository: {Name} tried to add a null entity", this.GetType().Name);
                throw;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, "Repository: {Name} failed when trying to add entity of type {Type}", this.GetType().Name, typeof(T).Name);
                throw;
            }
        }

        public virtual async Task AddBatch(IEnumerable<T> entities, int batchSize, IProgress<ProgressReport>? progress)
        {
            Guard.Against.Null(entities, nameof(entities));
            Guard.Against.NegativeOrZero(batchSize, nameof(batchSize));

            var entityList = entities.ToList();
            int total = entityList.Count;
            string message = $"Saving {total} {typeof(T).Name}";
            int count = 0;
            foreach (T entity in entityList)
            {
                await this.Add(entity, false);
                count++;
                if (count % batchSize == 0)
                {
                    await this.Commit();
                }
                progress?.Report(new ProgressReport { Message = message, TotalProgress = total, CurrentProgress = count });
            }
            if (count % batchSize != 0)
            {
                await this.Commit();
            }
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

        /// <summary>
        /// Deletes all entities matching the condition
        /// </summary>
        /// <returns>true if at least one entity was deleted, false if nothing matched</returns>
        public virtual async Task<bool> Delete(Expression<Func<T, bool>> where, bool commit = true)
        {
            try
            {
                List<T> objects = await dbset.Where(where).ToListAsync().ConfigureAwait(false);
                if (objects.Count == 0)
                {
                    this.logger.LogInformation("Repository: {Name} no entities of type {Type} matched delete condition", this.GetType().Name, typeof(T).Name);
                    return false;
                }

                dbset.RemoveRange(objects);
                if (commit)
                {
                    await dataContext.SaveChangesAsync().ConfigureAwait(false);
                }
                this.logger.LogInformation("Repository: {Name} deleted {Count} entities of type {Type}", this.GetType().Name, objects.Count, typeof(T).Name);
                return true;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError(e, "Repository: {Name} failed when trying to delete multiple entities of type {Type}", this.GetType().Name, typeof(T).Name);
                throw;
            }
        }

        /// <summary>
        /// Updates the entity, setting LastUpdated. Created and CreatedBy are never overwritten.
        /// </summary>
        public virtual async Task<T> Update(T entity, bool commit = true)
        {
            try
            {
                Guard.Against.Null(entity, nameof(entity));
                entity.LastUpdated = DateTime.UtcNow;
                var entry = dataContext.Entry(entity);
                entry.State = EntityState.Modified;
                entry.Property(nameof(IModel.Created)).IsModified = false;
                entry.Property(nameof(IModel.CreatedBy)).IsModified = false;
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
