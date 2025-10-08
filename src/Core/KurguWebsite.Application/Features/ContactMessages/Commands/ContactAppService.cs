using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.Contracts.Contact;
using KurguWebsite.Application.Features.ContactMessages.Commands;
using MediatR;

namespace KurguWebsite.Application.Features.ContactMessages
{
    internal sealed class ContactAppService : IContactAppService
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateContactMessageCommand> _validator;

        public ContactAppService(IMediator mediator, IMapper mapper, IValidator<CreateContactMessageCommand> validator)
        {
            _mediator = mediator;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<Result<object>> SubmitAsync(ContactMessageRequest request, CancellationToken ct = default)
        {
            // Map contract -> command (configure in Application mapping profile or inline here)
            var cmd = _mapper.Map<CreateContactMessageCommand>(request);

            // Validate with your Application-layer FluentValidation rules
            var vr = await _validator.ValidateAsync(cmd, ct);
            if (!vr.IsValid)
            {
                var errors = vr.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Result<object>.Failure("Validation failed.");
            }

            // Execute use case
            var result = await _mediator.Send(cmd, ct);
            // Ensure the result is of type Result<object>
            return Result<object>.Success(result);
        }
    }
}
