// Areas/Admin/Controllers/AuditLogsController.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.AuditLogs.Queries;
using KurguWebsite.WebUI.Areas.Admin.AdminControllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.AuditLogs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.Controllers
{
    public class AuditLogsController : BaseAdminController
    {
        public AuditLogsController(
           IMediator mediator,
           ILogger<AuditLogsController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/AuditLogs
        [HttpGet]
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            string? entityType = null,
            string? action = null,
            string? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? sortBy = null,
            string? sortOrder = null)
        {
            GetBreadcrumbs(("Audit Logs", null));

            var query = new SearchAuditLogsQuery
            {
                Params = new Application.Common.Models.QueryParameters
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = search,
                    SortColumn = sortBy,
                    SortOrder = sortOrder
                },
                UserId = userId,
                EntityType = entityType,
                Action = action,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to load audit logs");
                return View(new AuditLogIndexViewModel());
            }

            var viewModel = MapToPagedViewModel(
                result.Data,
                dto => new AuditLogListItemViewModel
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    UserName = dto.UserName,
                    Action = dto.Action,
                    EntityType = dto.EntityType,
                    EntityId = dto.EntityId,
                    IpAddress = dto.IpAddress,
                    Timestamp = dto.Timestamp
                },
                search,
                sortBy,
                sortOrder == "desc"
            );

            var indexViewModel = new AuditLogIndexViewModel
            {
                Items = viewModel.Items,
                PageNumber = viewModel.PageNumber,
                PageSize = viewModel.PageSize,
                TotalCount = viewModel.TotalCount,
                TotalPages = viewModel.TotalPages,
                HasPreviousPage = viewModel.HasPreviousPage,
                HasNextPage = viewModel.HasNextPage,
                SearchTerm = viewModel.SearchTerm,
                SortBy = viewModel.SortBy,
                SortDescending = viewModel.SortDescending,
                EntityTypes = GetEntityTypes(),
                Actions = GetActions(),
                SelectedEntityType = entityType,
                SelectedAction = action,
                SelectedUserId = userId,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(indexViewModel);
        }

       
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            GetBreadcrumbs(
                ("Audit Logs", Url.Action("Index")),
                ("Details", null)
            );

            var query = new GetAuditLogByIdQuery { Id = id };
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Audit log not found");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new AuditLogDetailsViewModel
            {
                Id = result.Data.Id,
                UserId = result.Data.UserId,
                UserName = result.Data.UserName,
                Action = result.Data.Action,
                EntityType = result.Data.EntityType,
                EntityId = result.Data.EntityId,
                OldValues = result.Data.OldValues,
                NewValues = result.Data.NewValues,
                IpAddress = result.Data.IpAddress,
                Timestamp = result.Data.Timestamp,
                OldValuesDict = ParseJsonToDictionary(result.Data.OldValues),
                NewValuesDict = ParseJsonToDictionary(result.Data.NewValues)
            };

            return View(viewModel);
        }

        // GET: Admin/AuditLogs/User/{userId}
        [HttpGet]
        public async Task<IActionResult> UserLogs(string userId, int pageNumber = 1, int pageSize = 20)
        {
            GetBreadcrumbs(
                ("Audit Logs", Url.Action("Index")),
                ("User Logs", null)
            );

            var query = new GetAuditLogsByUserQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to load user logs");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new AuditLogIndexViewModel
            {
                Items = result.Data.Select(dto => new AuditLogListItemViewModel
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    UserName = dto.UserName,
                    Action = dto.Action,
                    EntityType = dto.EntityType,
                    EntityId = dto.EntityId,
                    IpAddress = dto.IpAddress,
                    Timestamp = dto.Timestamp
                }).ToList(),
                SelectedUserId = userId
            };

            ViewData["PageTitle"] = $"Audit Logs - {result.Data.FirstOrDefault()?.UserName ?? userId}";

            return View("Index", viewModel);
        }

        // GET: Admin/AuditLogs/Export
        [HttpGet]
        public async Task<IActionResult> Export(
            string? entityType = null,
            string? action = null,
            string? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = new SearchAuditLogsQuery
            {
                Params = new Application.Common.Models.QueryParameters
                {
                    PageNumber = 1,
                    PageSize = 10000 // Large number for export
                },
                UserId = userId,
                EntityType = entityType,
                Action = action,
                FromDate = fromDate,
                ToDate = toDate
            };

            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetErrorMessage("Failed to export audit logs");
                return RedirectToAction(nameof(Index));
            }

            // Create CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,User,Action,EntityType,EntityId,IpAddress");

            foreach (var log in result.Data.Items)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.UserName},{log.Action},{log.EntityType},{log.EntityId},{log.IpAddress}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }

        #region Helper Methods

        private List<string> GetEntityTypes()
        {
            return new List<string>
            {
                "Service",
                "CaseStudy",
                "Testimonial",
                "Partner",
                "Page",
                "ProcessStep",
                "CompanyInfo",
                "ContactMessage",
                "User"
            };
        }

        private List<string> GetActions()
        {
            return new List<string>
            {
                "Create",
                "Update",
                "Delete",
                "Restore",
                "Login",
                "Logout",
                "View"
            };
        }

        private Dictionary<string, object>? ParseJsonToDictionary(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}