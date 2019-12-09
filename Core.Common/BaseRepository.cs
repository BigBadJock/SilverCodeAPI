using Core.Common.Contracts;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class, IModel, new()
    {

        private DbContext dataContext;
        private readonly DbSet<T> dbset;
        protected readonly ILog log;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataContext"></param>
         protected BaseRepository(DbContext dataContext)
        {
            log.Info($"Creating Repository {this.GetType().Name}");
            this.dataContext = dataContext;
            dbset = DataContext.Set<T>();
        }

        protected DbContext DataContext
        {
            get { return dataContext; }
        }

        public DbSet<T> DbSet => dbset;

        public virtual async Task<T> Add(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException(nameof(entity));
                entity.LastUpdated = DateTime.Now;
                entity.Created = DateTime.Now;
                T added = dbset.Add(entity);
                _ = await dataContext.SaveChangesAsync().ConfigureAwait(false);
                log.Info($"Repository: {this.GetType().Name} added new entity: {JsonConvert.SerializeObject(added)}");
                return added;
            }
            catch (ArgumentNullException)
            {
                log.Error($"Repository: {this.GetType().Name} tried to add a null entity {nameof(entity)}");
                throw;
            }
            catch (DbUpdateException e)
            {
                log.Error($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to add an entity {nameof(entity)} with the value: {JsonConvert.SerializeObject(entity)}", e);
                throw;
            }
        }

        public virtual async Task<bool> Delete(T entity)
        {
            try
            {
                dbset.Remove(entity);
                await dataContext.SaveChangesAsync().ConfigureAwait(false);
                log.Info($"Repository: {this.GetType().Name} deleted then entity: {JsonConvert.SerializeObject(entity)}");

                return true;
            }
            catch (DbUpdateException e)
            {
                log.Error($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to delete an entity : {JsonConvert.SerializeObject(entity)}", e);
                return false;
            }

        }

        public virtual async Task<bool> Delete(Expression<Func<T, bool>> where)
        {
            try
            {
                IEnumerable<T> objects = dbset.Where<T>(where).AsEnumerable();
                foreach (T obj in objects)
                {
                    dbset.Remove(obj);
                    log.Info($"Repository: {this.GetType().Name} deleting entity: {JsonConvert.SerializeObject(obj)}");
                }
                await dataContext.SaveChangesAsync().ConfigureAwait(false);
                log.Info($"Repository: {this.GetType().Name} deleting multiple entities successful");
                return true;
            }
            catch (DbUpdateException e)
            {
                log.Error($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to deleting multiple entries", e);
                throw;
            }
        }

        public virtual IQueryable<T> GetAll()
        {
            return dbset;
        }

        public virtual async Task<T> GetById(long id)
        {
            T result = await dbset.Where(s => s.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
            log.Info($"Repository: {this.GetType().Name} retrieving by Id: {id} value: {JsonConvert.SerializeObject(result)}");

            return result;
        }

        public virtual async Task<T> Update(T entity)
        {
            try
            {
                dbset.Attach(entity);
                dataContext.Entry(entity).State = EntityState.Modified;
                await dataContext.SaveChangesAsync().ConfigureAwait(false);
                log.Info($"Repository: {this.GetType().Name} updating entity: {JsonConvert.SerializeObject(entity)}");
                return entity;
            }
            catch (DbUpdateException e)
            {
                log.Error($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to update entity: {JsonConvert.SerializeObject(entity)}", e);
                throw;
            }
        }


    }
}
