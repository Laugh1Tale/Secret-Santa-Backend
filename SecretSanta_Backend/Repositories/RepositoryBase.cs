using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecretSanta_Backend.Interfaces;

namespace SecretSanta_Backend.Repositories
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected ApplicationContext context { get; set; }
        public RepositoryBase(ApplicationContext context)
        {
            this.context = context;
        }
        public IQueryable<T> FindAll() => context.Set<T>().AsNoTracking();
        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression) =>
            context.Set<T>().Where(expression).AsNoTracking();
        public void Create(T entity) => context.Set<T>().Add(entity);
        public void Update(T entity) => context.Set<T>().Update(entity);
        public void Delete(T entity) => context.Set<T>().Remove(entity);
    }
}
