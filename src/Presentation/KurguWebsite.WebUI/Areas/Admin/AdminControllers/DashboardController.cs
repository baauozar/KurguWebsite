using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.AuditLogs.Queries;
using KurguWebsite.Application.Features.CaseStudies.Queries;
using KurguWebsite.Application.Features.ContactMessages.Queries;
using KurguWebsite.Application.Features.Partners.Queries;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.Application.Features.Testimonials.Queries;
using KurguWebsite.WebUI.Areas.Admin.Controllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.AuditLogs;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.AdminControllers
{
    public class DashboardController : BaseAdminController
    {
        public DashboardController(
           IMediator mediator,
           ILogger<DashboardController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            GetBreadcrumbs(("Dashboard", null));

            var viewModel = new DashboardViewModel
            {
                Statistics = await GetStatistics(),
                RecentMessages = await GetRecentMessages(),
                RecentActivities = await GetRecentActivities()
            };

            return View(viewModel);
        }

        #region Helper Methods

        private async Task<DashboardStatistics> GetStatistics()
        {
            var statistics = new DashboardStatistics();

            try
            {
                // Get services stats
                var servicesQuery = new GetAllServicesQuery();
                var servicesResult = await Mediator.Send(servicesQuery);
                if (servicesResult.Succeeded && servicesResult.Data != null)
                {
                    statistics.TotalServices = servicesResult.Data.Count;
                    statistics.ActiveServices = servicesResult.Data.Count(s => s.IsActive);
                }

                // Get case studies stats
                var caseStudiesQuery = new GetFeaturedCaseStudiesQuery();
                var caseStudiesResult = await Mediator.Send(caseStudiesQuery);
                if (caseStudiesResult.Succeeded && caseStudiesResult.Data != null)
                {
                    statistics.TotalCaseStudies = caseStudiesResult.Data.Count;
                    statistics.FeaturedCaseStudies = caseStudiesResult.Data.Count(c => c.IsFeatured);
                }

                // Get testimonials stats
                var testimonialsQuery = new GetActiveTestimonialsQuery();
                var testimonialsResult = await Mediator.Send(testimonialsQuery);
                if (testimonialsResult.Succeeded && testimonialsResult.Data != null)
                {
                    statistics.TotalTestimonials = testimonialsResult.Data.Count;
                }

                // Get partners stats
                var partnersQuery = new GetAllPartnersQuery();
                var partnersResult = await Mediator.Send(partnersQuery);
                if (partnersResult.Succeeded && partnersResult.Data != null)
                {
                    statistics.TotalPartners = partnersResult.Data.Count;
                }

                // Get contact messages stats
                var messagesQuery = new GetPaginatedContactMessagesQuery
                {
                    PageNumber = 1,
                    PageSize = 100
                };
                var messagesResult = await Mediator.Send(messagesQuery);
                if (messagesResult.Succeeded && messagesResult.Data != null)
                {
                    statistics.UnreadMessages = messagesResult.Data.Items.Count(m => !m.IsRead);
                    statistics.UnrepliedMessages = messagesResult.Data.Items.Count(m => !m.IsReplied);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading dashboard statistics");
            }

            return statistics;
        }

        private async Task<List<RecentContactMessage>> GetRecentMessages()
        {
            try
            {
                var query = new GetPaginatedContactMessagesQuery
                {
                    PageNumber = 1,
                    PageSize = 5
                };

                var result = await Mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    return result.Data.Items.Select(m => new RecentContactMessage
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Subject = m.Subject,
                        CreatedDate = m.CreatedDate,
                        IsRead = m.IsRead
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading recent messages");
            }

            return new List<RecentContactMessage>();
        }

        private async Task<List<RecentActivityViewModel>> GetRecentActivities()
        {
            try
            {
                var query = new GetRecentAuditLogsQuery { Count = 10 };
                var result = await Mediator.Send(query);

                if (result.Succeeded && result.Data != null)
                {
                    return result.Data.Select(log => new RecentActivityViewModel
                    {
                        Action = log.Action,
                        EntityType = log.EntityType,
                        EntityId = log.EntityId,
                        UserName = log.UserName,
                        Timestamp = log.Timestamp
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error loading recent activities");
            }

            return new List<RecentActivityViewModel>();
        }

        #endregion
    }
}
