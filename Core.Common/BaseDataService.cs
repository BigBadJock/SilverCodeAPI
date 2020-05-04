using Core.Common.Contracts;
using Core.Common.DataModels.Interfaces;
using Microsoft.Extensions.Logging;
using REST_Parser.Models;
using System;
using System.Threading.Tasks;

namespace Core.Common
{
    public class BaseDataService<T> : IDataService<T> where T : class, IModel, new()
    {
        protected IRepository<T> repository;
        protected ILogger<T> logger;


        public async Task<T> Add(T model)
        {
            return await this.repository.Add(model);
        }

        public async Task<bool> Delete(T model)
        {
            return await this.repository.Delete(model);
        }

        public async Task<T> Get(int id)
        {
            return await this.repository.GetById(id);
        }

        public RestResult<T> Search(string restQuery)
        {
            this.logger.LogInformation(string.Format("searching using restQuery {0}", restQuery));
            return this.repository.GetAll(restQuery);
        }

        public Task<T> Update(T model)
        {
            throw new NotImplementedException();
        }
    }
}
