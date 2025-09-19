using AutoMapper;
using AutoMapper.QueryableExtensions;
using KurguWebsite.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Mappings
{
    public static class MappingExtensions
    {
        public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(
            this IQueryable<TDestination> queryable,
            int pageNumber,
            int pageSize)
        {
            return PaginatedList<TDestination>.CreateAsync(queryable, pageNumber, pageSize);
        }

        public static Task<List<TDestination>> ProjectToListAsync<TDestination>(
            this IQueryable queryable,
            IConfigurationProvider configuration)
        {
            return queryable.ProjectTo<TDestination>(configuration).ToListAsync();
        }
    }
}