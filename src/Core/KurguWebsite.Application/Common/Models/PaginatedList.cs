using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = count;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        // FIX: Keep synchronous method for in-memory collections
        public static PaginatedList<T> Create(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(items, count, pageNumber, pageSize);
        }

        // FIX: This is for in-memory collections only
        // If you need EF Core async, use the repository layer to return Task<PaginatedList<T>>
        public static Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
            => Task.FromResult(Create(source, pageNumber, pageSize));

        // ADD: Static factory for already-materialized lists (in-memory)
        public static PaginatedList<T> CreateFromList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}