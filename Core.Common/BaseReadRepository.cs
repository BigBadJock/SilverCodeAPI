using Core.Common.Contracts;
using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;
using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Common
{
    public abstract class BaseReadRepository<DBC, T> : IReadRepository<DBC, T>, IDisposable, IAsyncDisposable
        where T : class, IModel, new()
        where DBC : DbContext
    {

        protected readonly DbContext dataContext; // data context
        protected readonly ILogger<IReadRepository<DBC, T>> logger;
        protected readonly IRestToLinqParser<T> restParser;
        protected readonly DbSet<T> dbset;
        protected List<string> includes = [];
        private bool disposed;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dbcFactory">factory used to create the DbContext owned by this repository</param>
        protected BaseReadRepository(IDbContextFactory<DBC> dbcFactory, IRestToLinqParser<T> parser, ILogger<IReadRepository<DBC, T>> logger)
        {
            this.logger = logger;
            this.logger.LogInformation("Creating Repository {Name}", GetType().Name);
            this.dataContext = dbcFactory.CreateDbContext();
            this.restParser = parser;
            dbset = DataContext.Set<T>();

            GetIncludes();
        }

        public bool AlwaysIncludeChildren { get; set; }

        /// <summary>
        /// Collects the navigation properties of T from the EF Core model, for use with Include()
        /// </summary>
        private void GetIncludes()
        {
            var entityType = dataContext.Model.FindEntityType(typeof(T));
            if (entityType == null)
            {
                logger.LogWarning("Repository: {Name} entity type {Type} is not part of the model", GetType().Name, typeof(T).Name);
                return;
            }

            this.includes = entityType.GetNavigations().Select(n => n.Name)
                .Concat(entityType.GetSkipNavigations().Select(n => n.Name))
                .ToList();

            logger.LogInformation("Repository: {Name} navigation properties: {Includes}", GetType().Name, string.Join(", ", includes));
        }

        protected DbContext DataContext
        {
            get { return dataContext; }
        }

        public DbSet<T> DbSet => dbset;

        public virtual IQueryable<T> GetAll()
        {
            var dbResult = GetAllData();

            return dbResult;
        }

        public ApiResult<T> GetAll(string restQuery)
        {
            this.logger.LogInformation("Repository: {Name} running restQuery: {Query}", GetType().Name, restQuery);

            var dbResult = GetAllData();

            RestResult<T> restResult = this.restParser.Run(dbResult, restQuery);

            return new ApiResult<T>
            {
                Data = restResult.Data.ToList(),
                Pagination = restResult.PageSize > 0
                    ? new Pagination { PageSize = restResult.PageSize, PageNumber = restResult.Page, PageCount = restResult.PageCount, TotalCount = restResult.TotalCount }
                    : null
            };
        }

        private IQueryable<T> GetAllData(bool includeForCall = false)
        {
            var dbResult = dbset.AsQueryable();
            if (this.AlwaysIncludeChildren || includeForCall)
            {
                foreach (var inc in this.includes)
                {
                    dbResult = dbResult.Include(inc);
                }
            }
            return dbResult;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                dataContext.Dispose();
            }
            disposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            if (!disposed)
            {
                await dataContext.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
