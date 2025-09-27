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
    public static class EfQueryableExtensions
    {
        public static async Task<PaginatedList<T>> ToPaginatedListAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var count = await query.CountAsync(ct);
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }

        public static Task<List<TDestination>> ProjectToListAsync<TDestination>(
            this IQueryable query,
            IConfigurationProvider config,
            CancellationToken ct = default)
        {
            return query.ProjectTo<TDestination>(config).ToListAsync(ct);
        }
    }
}