using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SecretSanta_Backend.Interfaces
{
    public interface IRepositoryBase<T> where T: class
    {
        IQueryable<T> FindAll();
        IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression);
        void Create(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
