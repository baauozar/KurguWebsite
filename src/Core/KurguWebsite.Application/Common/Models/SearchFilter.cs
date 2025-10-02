using KurguWebsite.Application.DTOs.Service;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public class SearchFilter
    {
        public string? SearchTerm { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public List<string>? Categories { get; set; }
        public List<string>? Tags { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
    }

    // Usage in query:
    public class AdvancedSearchServicesQuery : IRequest<Result<PaginatedList<ServiceDto>>>
    {
        public SearchFilter Filter { get; set; } = new();
    }
}