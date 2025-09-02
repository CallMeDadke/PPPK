namespace MedicalSystemApp.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MedicalContext _ctx;
        private readonly IRepositoryFactory _factory;

        public UnitOfWork(MedicalContext ctx, IRepositoryFactory factory)
        {
            _ctx = ctx;
            _factory = factory;
        }

        public IRepository<T> Repository<T>() where T : class => _factory.GetRepository<T>();
        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _ctx.SaveChangesAsync(ct);
        public void Dispose() => _ctx.Dispose();
    }
}
