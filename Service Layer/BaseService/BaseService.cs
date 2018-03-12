using DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Service_Layer.BaseService
{
    public interface IBaseService<T> where T : class
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
        Task<IEnumerable<T>> GetAllAsync(string[] includedProperties);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter, string[] includedProperties);

        Task<IEnumerable<T>> PageAllAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, string[] includedProperties, int? pageNumber, int? pageSize);
        Task<IEnumerable<T>> PageAllAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, string[] includedProperties, int? pageNumber, int? pageSize);

        Task<int> EntitiesCountAsync(Expression<Func<T, bool>> filter);

        Task<T> FindByIdAsync(int id);
        Task<T> FindByIdAsync(CancellationToken cancellationToken, int id);

        Task<T> FindByPredicateAsync(Expression<Func<T, bool>> predicate);
        Task<T> FindByPredicateAsync(Expression<Func<T, bool>> predicate, string[] includedProperties);
        Task<T> FindByPredicateAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        Task UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task DeleteAsync(int id);
    }

    public abstract class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly IApplicationDbContext context;
        public BaseService(IApplicationDbContext ctx)
        {
            context = ctx;
        }

        public virtual async Task AddAsync(T entity)
        {
            context.Set<T>().Add(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await context.Set<T>().FindAsync(id);
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<T> FindByIdAsync(int id)
        {
            return await context.Set<T>().FindAsync(id);
        }

        public async Task<T> FindByIdAsync(CancellationToken cancellationToken, int id)
        {
            return await context.Set<T>().FindAsync(cancellationToken, id);
        }

        public async Task<T> FindByPredicateAsync(Expression<Func<T, bool>> predicate)
        {
            return await context.Set<T>().Where(predicate).FirstOrDefaultAsync();
        }

        public virtual async Task<T> FindByPredicateAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> predicate)
        {
            return await context.Set<T>().Where(predicate).FirstOrDefaultAsync(cancellationToken);
        }

        public IEnumerable<T> GetAll()
        {
            return context.Set<T>().ToList();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter)
        {
            return context.Set<T>().Where(filter).ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken)
        {
            return await context.Set<T>().ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<T>> PageAllAsync(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, string[] includedProperties, int? pageNumber, int? pageSize)
        {
            IQueryable<T> query = context.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includedProperties != null)
            {
                for (int i = 0; i < includedProperties.Count(); i++)
                {
                    query = query.Include(includedProperties[i]);
                }
            }


            if (orderBy != null)
            {
                query = orderBy(query);

                if (pageNumber != null)
                {
                    query = query.Skip(((int)pageNumber - 1) * (int)pageSize);
                }

                if (pageSize != null)
                {
                    query = query.Take((int)pageSize);
                }
            }
            else
            {
                throw new ArgumentNullException("Parameter can't be null", "orderBy");
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> PageAllAsync(CancellationToken cancellationToken, Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, string[] includedProperties, int? pageNumber, int? pageSize)
        {
            IQueryable<T> query = context.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includedProperties != null)
            {
                for (int i = 0; i < includedProperties.Count(); i++)
                {
                    query = query.Include(includedProperties[i]);
                }
            }


            if (orderBy != null)
            {
                query = orderBy(query);

                if (pageNumber != null)
                {
                    query = query.Skip(((int)pageNumber - 1) * (int)pageSize);
                }

                if (pageSize != null)
                {
                    query = query.Take((int)pageSize);
                }
            }
            else
            {
                throw new ArgumentNullException("Parameter can't be null", "orderBy");
            }

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task<int> EntitiesCountAsync(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = context.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.CountAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(string[] includedProperties)
        {
            IQueryable<T> query = context.Set<T>();
            if (includedProperties != null)
            {
                for (int i = 0; i < includedProperties.Count(); i++)
                {
                    query = query.Include(includedProperties[i]);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
            return await context.Set<T>().Where(filter).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> filter, string[] includedProperties)
        {
            IQueryable<T> query = context.Set<T>();
            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (includedProperties != null)
            {
                for (int i = 0; i < includedProperties.Count(); i++)
                {
                    query = query.Include(includedProperties[i]);
                }
            }
            return await query.ToListAsync();
        }

        public async Task<T> FindByPredicateAsync(Expression<Func<T, bool>> predicate, string[] includedProperties)
        {
            IQueryable<T> query = context.Set<T>();
            if (includedProperties != null)
            {
                for (int i = 0; i < includedProperties.Count(); i++)
                {
                    query = query.Include(includedProperties[i]);
                }
            }
            return await query.Where(predicate).SingleOrDefaultAsync();
        }
    }
}