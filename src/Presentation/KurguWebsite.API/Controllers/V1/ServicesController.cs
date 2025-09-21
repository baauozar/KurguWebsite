using KurguWebsite.Application.Common.Interfaces.Services;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.WebAPI.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
namespace KurguWebsite.API.Controllers.V1
{
    [ApiVersion("1.0")]
    [EnableRateLimiting("ApiLimit")]
    public class ServicesController : BaseApiController
    {
        private readonly IServiceManagementService _serviceService;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(
            IServiceManagementService serviceService,
            ILogger<ServicesController> logger)
        {
            _serviceService = serviceService;
            _logger = logger;
        }

        /// <summary>
        /// Get all services
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServices()
        {
            var result = await _serviceService.GetActiveServicesAsync();

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get featured services
        /// </summary>
        [HttpGet("featured")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(IEnumerable<ServiceDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeaturedServices()
        {
            var result = await _serviceService.GetFeaturedServicesAsync();

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get service by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetService(Guid id)
        {
            var result = await _serviceService.GetByIdAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get service by slug
        /// </summary>
        [HttpGet("slug/{slug}")]
        [AllowAnonymous]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(ServiceDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetServiceBySlug(string slug)
        {
            var result = await _serviceService.GetServiceDetailAsync(slug);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// Get paginated services
        /// </summary>
        [HttpGet("paginated")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaginatedServices(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _serviceService.GetPaginatedAsync(pageNumber, pageSize);

            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(new
            {
                result.Data?.Items,
                result.Data?.PageNumber,
                result.Data?.TotalPages,
                result.Data?.TotalCount,
                result.Data?.HasPreviousPage,
                result.Data?.HasNextPage
            });
        }

        /// <summary>
        /// Create new service (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceDto dto)
        {
            _logger.LogInformation("Creating new service: {Title}", dto.Title);

            var result = await _serviceService.CreateAsync(dto);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Failed to create service: {Errors}", string.Join(", ", result.Errors));
                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation("Service created successfully with ID: {Id}", result.Data?.Id);
            return CreatedAtAction(nameof(GetService), new { id = result.Data?.Id }, result.Data);
        }

        /// <summary>
        /// Update service (Admin only)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ServiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] UpdateServiceDto dto)
        {
            _logger.LogInformation("Updating service: {Id}", id);

            var result = await _serviceService.UpdateAsync(id, dto);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Contains("not found", StringComparison.OrdinalIgnoreCase)))
                    return NotFound(new { message = result.Message });

                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation("Service updated successfully: {Id}", id);
            return Ok(result.Data);
        }

        /// <summary>
        /// Delete service (Admin only)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            _logger.LogInformation("Deleting service: {Id}", id);

            var result = await _serviceService.DeleteAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            _logger.LogInformation("Service deleted successfully: {Id}", id);
            return NoContent();
        }

        /// <summary>
        /// Toggle service status (Admin only)
        /// </summary>
        [HttpPatch("{id:guid}/toggle-status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleServiceStatus(Guid id)
        {
            var result = await _serviceService.ToggleStatusAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Toggle service featured status (Admin only)
        /// </summary>
        [HttpPatch("{id:guid}/toggle-featured")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleServiceFeatured(Guid id)
        {
            var result = await _serviceService.ToggleFeaturedAsync(id);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Update service display order (Admin only)
        /// </summary>
        [HttpPatch("{id:guid}/display-order")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDisplayOrder(Guid id, [FromBody] int displayOrder)
        {
            var result = await _serviceService.UpdateDisplayOrderAsync(id, displayOrder);

            if (!result.Succeeded)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message });
        }
    }
}
