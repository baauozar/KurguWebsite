// Areas/Admin/Controllers/BaseController.cs
using KurguWebsite.Application.Common.Models;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.SharedAdmin;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.AdminControllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IMediator Mediator;
        protected readonly ILogger Logger;

        protected BaseController(IMediator mediator, ILogger logger)
        {
            Mediator = mediator;
            Logger = logger;
        }

        #region Success/Error Messages

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }

        protected void SetWarningMessage(string message)
        {
            TempData["WarningMessage"] = message;
        }

        protected void SetInfoMessage(string message)
        {
            TempData["InfoMessage"] = message;
        }

        #endregion

        #region Result Handling

        protected IActionResult HandleResult<T>(Result<T> result, string successMessage, string? successRedirectAction = null, string? successRedirectController = null)
        {
            if (result.Succeeded)
            {
                SetSuccessMessage(successMessage);

                if (!string.IsNullOrEmpty(successRedirectAction))
                {
                    return RedirectToAction(successRedirectAction, successRedirectController);
                }

                return Ok(new { success = true, message = successMessage, data = result.Data });
            }

            var errorMessage = result.Errors.Any()
                ? string.Join(", ", result.Errors)
                : "An error occurred";

            SetErrorMessage(errorMessage);
            return BadRequest(new { success = false, errors = result.Errors });
        }

        protected IActionResult HandleControlResult(ControlResult result, string successMessage, string? successRedirectAction = null, string? successRedirectController = null)
        {
            if (result.Succeeded)
            {
                SetSuccessMessage(successMessage);

                if (!string.IsNullOrEmpty(successRedirectAction))
                {
                    return RedirectToAction(successRedirectAction, successRedirectController);
                }

                return Ok(new { success = true, message = successMessage });
            }

            var errorMessage = result.Errors.Any()
                ? string.Join(", ", result.Errors)
                : "An error occurred";

            SetErrorMessage(errorMessage);
            return BadRequest(new { success = false, errors = result.Errors });
        }

        #endregion

        #region Pagination Helpers

        protected PagedViewModel<TViewModel> MapToPagedViewModel<TDto, TViewModel>(
            PaginatedList<TDto> source,
            Func<TDto, TViewModel> mapper,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            return new PagedViewModel<TViewModel>
            {
                Items = source.Items.Select(mapper).ToList(),
                PageNumber = source.PageNumber,
                PageSize = source.Items.Count,
                TotalCount = source.TotalCount,
                TotalPages = source.TotalPages,
                HasPreviousPage = source.HasPreviousPage,
                HasNextPage = source.HasNextPage,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDescending = sortDescending
            };
        }

        #endregion

        #region File Upload Helpers

        protected async Task<string?> SaveUploadedFile(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", folderName);
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/{folderName}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error uploading file");
                return null;
            }
        }

        protected void DeleteFile(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            }
        }

        #endregion

        #region Validation Helpers

        protected bool ValidateModel<T>(T model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                SetErrorMessage(string.Join(", ", errors));
                return false;
            }
            return true;
        }

        #endregion
    }
}