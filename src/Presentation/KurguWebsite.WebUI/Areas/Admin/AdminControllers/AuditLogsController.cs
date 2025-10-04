// Areas/Admin/Controllers/AuditLogsController.cs
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.AuditLogs.Queries;
using KurguWebsite.WebUI.Areas.Admin.AdminControllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.AuditLogs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace KurguWebsite.WebUI.Areas.Admin.Controllers
{
    public class AuditLogsController : BaseAdminController
    {
        public AuditLogsController(
           IMediator mediator,
           ILogger<AuditLogsController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService) { }

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
            string? sortBy = null,     // "Timestamp","UserName","EntityType","Action","EntityId"
            string? sortOrder = "desc" // "asc"/"desc"
        )
        {
            GetBreadcrumbs(("Audit Logs", null));

            var query = new SearchAuditLogsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                UserId = userId,
                EntityType = entityType,
                Action = action,
                FromDate = fromDate,
                ToDate = toDate,
                SearchTerm = search,
                SortColumn = sortBy,
                SortOrder = sortOrder
            };

            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data is null)
            {
                SetErrorMessage("Failed to load audit logs");
                return View(new AuditLogIndexViewModel());
            }

            var vm = new AuditLogIndexViewModel
            {
                Items = result.Data.Items.Select(dto => new AuditLogListItemViewModel
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

                PageNumber = result.Data.PageNumber,
                PageSize = pageSize,
                TotalCount = result.Data.TotalCount,
                TotalPages = result.Data.TotalPages,
                HasPreviousPage = result.Data.HasPreviousPage,
                HasNextPage = result.Data.HasNextPage,

                SearchTerm = search,
                SortBy = sortBy,
                SortDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase),
                EntityTypes = GetEntityTypes(),
                Actions = GetActions(),
                SelectedEntityType = entityType,
                SelectedAction = action,
                SelectedUserId = userId,
                FromDate = fromDate,
                ToDate = toDate
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            GetBreadcrumbs(("Audit Logs", Url.Action("Index")), ("Details", null));

            var result = await Mediator.Send(new GetAuditLogByIdQuery { Id = id });
            if (!result.Succeeded || result.Data is null)
            {
                SetErrorMessage("Audit log not found");
                return RedirectToAction(nameof(Index));
            }

            var vm = new AuditLogDetailsViewModel
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

            return View(vm);
        }

        // /Admin/AuditLogs/User?userId=...
        [HttpGet]
        public async Task<IActionResult> UserLogs(string userId, int pageNumber = 1, int pageSize = 20)
        {
            GetBreadcrumbs(("Audit Logs", Url.Action("Index")), ("User Logs", null));

            var result = await Mediator.Send(new SearchAuditLogsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                UserId = userId,
                SortColumn = "Timestamp",
                SortOrder = "desc"
            });

            if (!result.Succeeded || result.Data is null)
            {
                SetErrorMessage("Failed to load user logs");
                return RedirectToAction(nameof(Index));
            }

            var vm = new AuditLogIndexViewModel
            {
                Items = result.Data.Items.Select(dto => new AuditLogListItemViewModel
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

                PageNumber = result.Data.PageNumber,
                PageSize = pageSize,
                TotalCount = result.Data.TotalCount,
                TotalPages = result.Data.TotalPages,
                HasPreviousPage = result.Data.HasPreviousPage,
                HasNextPage = result.Data.HasNextPage,
                SelectedUserId = userId
            };

            ViewData["PageTitle"] = $"Audit Logs - {result.Data.Items.FirstOrDefault()?.UserName ?? userId}";
            return View("Index", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Export(
            string? entityType = null,
            string? action = null,
            string? userId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var result = await Mediator.Send(new SearchAuditLogsQuery
            {
                PageNumber = 1,
                PageSize = 10000, // simple: one big page; or loop pages for huge exports
                UserId = userId,
                EntityType = entityType,
                Action = action,
                FromDate = fromDate,
                ToDate = toDate,
                SortColumn = "Timestamp",
                SortOrder = "desc"
            });

            if (!result.Succeeded || result.Data is null)
            {
                SetErrorMessage("Failed to export audit logs");
                return RedirectToAction(nameof(Index));
            }

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,User,Action,EntityType,EntityId,IpAddress");

            foreach (var log in result.Data.Items)
            {
                csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.UserName},{log.Action},{log.EntityType},{log.EntityId},{log.IpAddress}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"audit-logs-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }

        #region Helpers

        private static List<string> GetEntityTypes() => new()
        {
            "Service","CaseStudy","Testimonial","Partner","Page",
            "ProcessStep","CompanyInfo","ContactMessage","User"
        };

        private static List<string> GetActions() => new()
        {
            "Create","Update","Delete","Restore","Login","Logout","View"
        };

        private static Dictionary<string, object>? ParseJsonToDictionary(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json); }
            catch { return null; }
        }

        #endregion
    }
}
