using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.Enums;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface IServiceRepository : IGenericRepository<Service>
    {
        Task<Service?> GetBySlugAsync(string slug);
        Task<Service?> GetServiceWithFeaturesAsync(Guid id);
        Task<IReadOnlyList<Service>> GetActiveServicesAsync();
        Task<IReadOnlyList<Service>> GetFeaturedServicesAsync();
        Task<IReadOnlyList<Service>> GetServicesByCategoryAsync(ServiceCategory category);
        Task<bool> SlugExistsAsync(string slug, Guid? excludeId = null);
        Task<Service?> GetBySlugWithFeaturesAsync(string slug);
        IQueryable<Service> GetActiveQueryable();
        IQueryable<Service> GetActiveServicesQueryable();

    }
}
