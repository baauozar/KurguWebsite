using KurguWebsite.Application.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;

namespace KurguWebsite.Application.Common.Interfaces.Repositories
{
    public interface ITestimonialRepository : IGenericRepository<Testimonial>
    {
        Task<IReadOnlyList<Testimonial>> GetActiveTestimonialsAsync();
        Task<IReadOnlyList<Testimonial>> GetFeaturedTestimonialsAsync();
        Task<Testimonial?> GetRandomTestimonialAsync();
    }
}
