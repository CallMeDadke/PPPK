using System.Linq.Expressions;

namespace MedicalSystemApp.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(object id);
        Task<List<T>> GetAllAsync();
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);
        IQueryable<T> Query();
    }
}
