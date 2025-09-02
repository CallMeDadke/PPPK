using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MedicalSystemApp.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly MedicalContext _ctx;
        private readonly DbSet<T> _set;

        public Repository(MedicalContext ctx)
        {
            _ctx = ctx;
            _set = _ctx.Set<T>();
        }

        public async Task<T?> GetByIdAsync(object id) => await _set.FindAsync(id);
        public async Task<List<T>> GetAllAsync() => await _set.ToListAsync();
        public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _set.Where(predicate).ToListAsync();
        public async Task AddAsync(T entity) => await _set.AddAsync(entity);
        public void Update(T entity) => _set.Update(entity);
        public void Remove(T entity) => _set.Remove(entity);
        public IQueryable<T> Query() => _set.AsQueryable();
    }
}
