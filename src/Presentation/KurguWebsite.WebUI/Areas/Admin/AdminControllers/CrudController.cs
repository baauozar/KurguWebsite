// Areas/Admin/Controllers/CrudController.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KurguWebsite.WebUI.Areas.Admin.Controllers
{
    public abstract class CrudController<TListVm, TDetailVm, TCreateVm, TUpdateVm, TId> : BaseAdminController
    {
        protected readonly IMediator _mediator;
        protected readonly IMapper _mapper;
        protected CrudController(
            IMediator mediator,
            IMapper mapper,
            ILogger logger,
            IPermissionService permissionService
        ) : base(mediator, logger, permissionService)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public abstract Task<IActionResult> Index([FromQuery] object? filter = null);
        public abstract Task<IActionResult> Details(TId id);

        [HttpGet] public abstract Task<IActionResult> Create();
        [HttpPost, ValidateAntiForgeryToken] public abstract Task<IActionResult> Create(TCreateVm vm);

        [HttpGet] public abstract Task<IActionResult> Edit(TId id);
        [HttpPost, ValidateAntiForgeryToken] public abstract Task<IActionResult> Edit(TId id, TUpdateVm vm);

        [HttpPost, ValidateAntiForgeryToken] public abstract Task<IActionResult> Delete(TId id);
    }
}
