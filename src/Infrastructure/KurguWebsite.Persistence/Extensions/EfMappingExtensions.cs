using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Persistence.Extensions
{
    public static class EfMappingExtensions
    {
        public static Task<List<TDestination>> ProjectToListAsync<TDestination>(
            this IQueryable query,
            IConfigurationProvider configuration,
            CancellationToken ct = default)
            => query.ProjectTo<TDestination>(configuration).ToListAsync(ct);

        public static async Task<PaginatedList<TDestination>> ToPaginatedListAsync<TSource, TDestination>(
            this IQueryable<TSource> query,
            IConfigurationProvider configuration,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var count = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<TDestination>(configuration)
                .ToListAsync(ct);

            return new PaginatedList<TDestination>(items, count, pageNumber, pageSize);
        }
    }
}