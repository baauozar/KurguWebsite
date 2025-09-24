using Asp.Versioning;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Application.Features.CompanyInfo.Commands;
using KurguWebsite.Application.Features.CompanyInfo.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CompanyInfoController : BaseApiController
    {
        private readonly IMediator _mediator;

        public CompanyInfoController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets the publicly available company information.
        /// </summary>
        /// <returns>The company's public information.</returns>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(CompanyInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            var result = await _mediator.Send(new GetCompanyInfoQuery());
            return result.Succeeded ? Ok(result.Data) : NotFound(result.Errors);
        }

        /// <summary>
        /// Updates the basic company information (name, about, mission, logos, etc.). (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the updated basic information.</param>
        /// <returns>The updated company information.</returns>
        [HttpPut("basic")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CompanyInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateBasicInfo(UpdateCompanyInfoBasicCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
        }

        /// <summary>
        /// Updates the company's contact information (phone numbers, email). (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the updated contact information.</param>
        /// <returns>The updated company information.</returns>
        [HttpPut("contact")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CompanyInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateContactInfo(UpdateCompanyInfoContactCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
        }

        /// <summary>
        /// Updates the company's physical address. (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the updated address information.</param>
        /// <returns>The updated company information.</returns>
        [HttpPut("address")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CompanyInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateAddress(UpdateCompanyInfoAddressCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
        }

        /// <summary>
        /// Updates the company's social media links. (Admin Only)
        /// </summary>
        /// <param name="command">The command containing the updated social media links.</param>
        /// <returns>The updated company information.</returns>
        [HttpPut("social-media")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(CompanyInfoDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateSocialMedia(UpdateCompanyInfoSocialMediaCommand command)
        {
            var result = await _mediator.Send(command);
            return result.Succeeded ? Ok(result.Data) : BadRequest(result.Errors);
        }
    }
}