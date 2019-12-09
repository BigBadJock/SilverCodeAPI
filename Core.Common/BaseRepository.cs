using Core.Common.Contracts;
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataContext"></param>
         protected BaseRepository(DbContext dataContext)
        {
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
                _ = await dataContext.SaveChangesAsync();
                return added;
            }
            catch (DbUpdateException e)
            {
                // TODO: Log this
                // TODO: Audit this
                System.Diagnostics.Debug.WriteLine(e.Message);
                if (e.InnerException != null) System.Diagnostics.Debug.WriteLine(e.InnerException.Message);
                return null;
            }
        }

        public virtual async Task<bool> Delete(T entity)
        {
            try
            {

                dbset.Remove(entity);
                await dataContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                // TODO: Log this
                // TODO: Audit this
                System.Diagnostics.Debug.WriteLine(e.Message);
                if (e.InnerException != null) System.Diagnostics.Debug.WriteLine(e.InnerException.Message);
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
                }
                await dataContext.SaveChangesAsync();
                return true;
            }
            catch (Exception e)
            {
                // TODO: Add Logging instead.
                // TODO: Audit this
                System.Diagnostics.Debug.WriteLine(e.Message);
                if (e.InnerException != null) System.Diagnostics.Debug.WriteLine(e.InnerException.Message);
                return false;
            }
        }

        public virtual IQueryable<T> GetAll()
        {
            return dbset;
        }

        public virtual async Task<T> GetById(long id)
        {
            return await dbset.Where(s => s.Id == id).FirstOrDefaultAsync();
        }

        public virtual async Task<T> Update(T entity)
        {
            try
            {
                dbset.Attach(entity);
                dataContext.Entry(entity).State = EntityState.Modified;
                await dataContext.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateException e)
            {
                // TODO: Log this
                // TODO: Audit this
                System.Diagnostics.Debug.WriteLine(e.Message);
                if (e.InnerException != null) System.Diagnostics.Debug.WriteLine(e.InnerException.Message);
                throw e;
            }
        }


    }
}
