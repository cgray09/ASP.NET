using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Uplift.DataAccess.Data.Repository.IRepository
{
    // "<T>" so and of the models can use it (like CategoryRepository)
    public interface IRepository<T> where T : class
    {
        T Get(int id);

        // "IEnumerable<T>" for a list
        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null // this is for eager loading
            );

        T GetFirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null
            );

        void Add(T entity);

        void Remove(int id);
        void Remove(T entity);
    }
}
