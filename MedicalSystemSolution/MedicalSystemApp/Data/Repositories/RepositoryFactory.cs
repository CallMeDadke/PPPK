using Microsoft.Extensions.DependencyInjection;

namespace MedicalSystemApp.Data.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IServiceProvider _sp;
        public RepositoryFactory(IServiceProvider sp) => _sp = sp;

        public IRepository<T> GetRepository<T>() where T : class
            => ActivatorUtilities.CreateInstance<Repository<T>>(_sp);
    }
}
