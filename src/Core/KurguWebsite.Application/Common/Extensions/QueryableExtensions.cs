using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PaginatedList<TDestination>> ToPaginatedListAsync<TSource, TDestination>(
            this IQueryable<TSource> query,
            IConfigurationProvider configuration,
            int pageNumber,
            int pageSize)
        {
            var count = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<TDestination>(configuration)
                .ToListAsync();

            return new PaginatedList<TDestination>(items, count, pageNumber, pageSize);
        }
    }
}