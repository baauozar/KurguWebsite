// Components/ServicesGrid/ServicesGridViewComponent.cs
using AutoMapper;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Features.Services.Queries;
using KurguWebsite.WebUI.UIModel.Service;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace KurguWebsite.WebUI.Components.ServicesGrid
{
    public class ServicesGridViewComponent : ViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ServicesGridViewComponent(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync(int take = 6)
        {
            var res = await _mediator.Send(new GetPaginatedServicesQuery
            {
                Params = new PaginationParams { PageNumber = 1, PageSize = take }
            });

            var items = (res.Succeeded && res.Data != null) ? res.Data.Items : Enumerable.Empty<ServiceDto>();
            var vms = items.Select(_mapper.Map<ServiceCardVm>).ToList();
            return View(vms);
        }
    }
}
