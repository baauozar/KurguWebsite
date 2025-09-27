using KurguWebsite.Application.Common.Interfaces.Repositories;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Repositories
{
    public class TestimonialRepository : GenericRepository<Testimonial>, ITestimonialRepository
    {
        public TestimonialRepository(KurguWebsiteDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<Testimonial>> GetActiveTestimonialsAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ThenByDescending(t => t.TestimonialDate)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Testimonial>> GetFeaturedTestimonialsAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive && t.IsFeatured)
                .OrderBy(t => t.DisplayOrder)
                .ThenByDescending(t => t.TestimonialDate)
                .ToListAsync();
        }

        public async Task<Testimonial?> GetRandomTestimonialAsync()
        {
            var activeTestimonials = await _dbSet
                .Where(t => t.IsActive)
                .ToListAsync();

            if (!activeTestimonials.Any())
                return null;

            var random = new Random();
            var index = random.Next(activeTestimonials.Count);
            return activeTestimonials[index];
        }
        public IQueryable<Testimonial> GetAllQueryable()
        {
            return _dbSet.AsNoTracking();
        }

        public async Task<IReadOnlyList<Testimonial>> GetAllActiveAsync()
        {
            return await _dbSet
                .Where(t => t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}