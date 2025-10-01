namespace KurguWebsite.WebUI.Areas.Admin.AdminModels.SharedAdmin
{
    public record PagedRequest(int PageNumber = 1, int PageSize = 10, string? Search = null, string? Sort = null);

    // Areas/Admin/Models/Shared/PagedResultVm.cs
    public class PagedResultVm<T>
    {
        public IReadOnlyList<T> Items { get; init; } = [];
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
    }
}
