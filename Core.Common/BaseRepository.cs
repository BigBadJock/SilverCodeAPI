using Ardalis.GuardClauses;
using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using REST_Parser;
using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class, IModel, new()
    {

        private DbContext dataContext;
        private ILogger<Common.BaseRepository<T>> logger;
        private readonly IRestToLinqParser<T> restParser;
        private readonly DbSet<T> dbset;
        private List<string> includes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataContext"></param>
         protected BaseRepository(DbContext dataContext, IRestToLinqParser<T> parser, ILogger<BaseRepository<T>> logger)
        {
            this.logger = logger;
            this.logger.LogInformation($"Creating Repository {this.GetType().Name}");
            this.dataContext = dataContext;
            this.restParser = parser;
            dbset = DataContext.Set<T>();

            var props = typeof(T).GetProperties().ToList();
            
            GetIncludes(props);
        }

        public bool IncludeChildren { get; set; }

        private void GetIncludes(List<PropertyInfo> props)
        {
            this.includes = new List<string>();
            props.ForEach(prop =>
            {
                try
                {
                    {

                        if (prop.PropertyType.IsGenericType)
                        {
                            this.logger.LogInformation($"adding property collection: {prop.Name}");
                            this.includes.Add(prop.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"error checking property type: {prop.PropertyType} error: {ex.Message}");

                }


            });
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
                Guard.Against.Null(entity, nameof(entity));
                entity.LastUpdated = DateTime.Now;
                entity.Created = DateTime.Now;
                var added = dbset.Add(entity);
                _ = await dataContext.SaveChangesAsync().ConfigureAwait(false);
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

        public virtual async Task<bool> Delete(T entity)
        {
            try
            {
                Guard.Against.Null(entity, nameof(entity));
                dbset.Remove(entity);
                await dataContext.SaveChangesAsync().ConfigureAwait(false);
                this.logger.LogInformation($"Repository: {this.GetType().Name} deleted then entity: {JsonConvert.SerializeObject(entity)}");

                return true;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to delete an entity : {JsonConvert.SerializeObject(entity)}", e);
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
                    this.logger.LogInformation($"Repository: {this.GetType().Name} deleting entity: {JsonConvert.SerializeObject(obj)}");
                }
                await dataContext.SaveChangesAsync().ConfigureAwait(false);
                this.logger.LogInformation($"Repository: {this.GetType().Name} deleting multiple entities successful");
                return true;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to deleting multiple entries", e);
                throw;
            }
        }

        public virtual IQueryable<T> GetAll()
        {
            var dbResult = getAllData();

            return dbResult;
        }

        public RestResult<T> GetAll(string restQuery)
        {
            this.logger.LogInformation($"Repository: {this.GetType().Name} running restQuery: {restQuery}");

            var dbResult = getAllData();

            RestResult<T> result = this.restParser.Run(dbResult, restQuery);


            return result;
        }

        public virtual async Task<T> GetById(Guid id)
        {
            var dbResult = getAllData();
            T result = await dbResult.Where(s => s.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);

            return result;
        }

        public virtual async Task<T> Update(T entity)
        {
            try
            {
                Guard.Against.Null(entity, nameof(entity));
                dbset.Attach(entity);
                dataContext.Entry(entity).State = EntityState.Modified;
                await dataContext.SaveChangesAsync().ConfigureAwait(false);
                this.logger.LogInformation($"Repository: {this.GetType().Name} updating entity: {JsonConvert.SerializeObject(entity)}");
                return entity;
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Repository: {this.GetType().Name} failed throwing exception: {e} when trying to update entity: {JsonConvert.SerializeObject(entity)}", e);
                throw;
            }
        }

        private IQueryable<T> getAllData()
        {
            var dbResult = dbset.AsQueryable();
            if (this.IncludeChildren)
            {
                foreach (var include in this.includes)
                {
                    dbResult = dbResult.Include(include);
                }
            }
            return dbResult;
        }
    }
}
