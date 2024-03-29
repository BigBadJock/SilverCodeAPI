﻿using Core.Common.Contracts;
using Core.Common.DataModels;
using Core.Common.DataModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using REST_Parser;
using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Core.Common
{
    public abstract class BaseReadRepository<DBC, T> : IReadRepository<DBC, T>
        where T : class, IModel, new()
        where DBC : DbContext
    {

        protected readonly DbContext dataContext; // data context
        protected readonly ILogger<IReadRepository<DBC, T>> logger;
        protected readonly IRestToLinqParser<T> restParser;
        protected readonly DbSet<T> dbset;
        protected List<string> includes;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataContext"></param>
        protected BaseReadRepository(IDbContextFactory<DBC> dbcFactory, IRestToLinqParser<T> parser, ILogger<IReadRepository<DBC, T>> logger)
        {
            this.logger = logger;
            this.logger.LogInformation($"Creating Repository {this.GetType().Name}");
            this.dataContext = dbcFactory.CreateDbContext();
            this.restParser = parser;
            dbset = DataContext.Set<T>();

            var props = typeof(T).GetProperties().ToList();

            GetIncludes(props);
        }

        public bool AlwaysIncludeChildren { get; set; }

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

        public virtual IQueryable<T> GetAll()
        {
            var dbResult = GetAllData();

            return dbResult;
        }

        public ApiResult<T> GetAll(string restQuery)
        {
            this.logger.LogInformation($"Repository: {this.GetType().Name} running restQuery: {restQuery}");

            var dbResult = GetAllData();

            RestResult<T> restResult = this.restParser.Run(dbResult, restQuery);

            ApiResult<T> apiResult = new ApiResult<T>();
            apiResult.Data = restResult.Data.ToList();
            if (restResult.PageSize > 0)
            {
                apiResult.Pagination = new Pagination { PageSize = restResult.PageSize, PageNumber = restResult.Page, PageCount = restResult.PageCount, TotalCount = restResult.TotalCount };
            }

            return apiResult;
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
    }
}
