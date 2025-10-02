using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Features.CompanyInfo.Commands;
using KurguWebsite.Application.Features.CompanyInfo.Queries;
using KurguWebsite.WebUI.Areas.Admin.Controllers;
using KurguWebsite.WebUI.Areas.Admin.ViewModel.CompanyInfo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KurguWebsite.WebUI.Areas.Admin.AdminControllers
{
    public class CompanyInfoController : BaseAdminController
    {
        public CompanyInfoController(
           IMediator mediator,
           ILogger<CompanyInfoController> logger,
           IPermissionService permissionService)
           : base(mediator, logger, permissionService)
        {
        }

        // GET: Admin/CompanyInfo
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            GetBreadcrumbs(("Company Information", null));

            var query = new GetCompanyInfoQuery();
            var result = await Mediator.Send(query);

            if (!result.Succeeded || result.Data == null)
            {
                SetWarningMessage("Company information not configured");
                return View(new CompanyInfoEditViewModel());
            }

            var viewModel = new CompanyInfoEditViewModel
            {
                Id = result.Data.Id,
                CompanyName = result.Data.CompanyName,
                LogoPath = result.Data.LogoPath,
                LogoLightPath = result.Data.LogoLightPath,
                About = result.Data.About,
                Mission = result.Data.Mission,
                Vision = result.Data.Vision,
                Slogan = result.Data.Slogan,
                CopyrightText = result.Data.CopyrightText,
                SupportPhone = result.Data.ContactInformation.SupportPhone,
                SalesPhone = result.Data.ContactInformation.SalesPhone,
                Email = result.Data.ContactInformation.Email,
                SupportEmail = result.Data.ContactInformation.SupportEmail,
                SalesEmail = result.Data.ContactInformation.SalesEmail,
                Street = result.Data.OfficeAddress.Street,
                Suite = result.Data.OfficeAddress.Suite,
                City = result.Data.OfficeAddress.City,
                State = result.Data.OfficeAddress.State,
                PostalCode = result.Data.OfficeAddress.PostalCode,
                Country = result.Data.OfficeAddress.Country,
                Facebook = result.Data.SocialMedia?.Facebook,
                Twitter = result.Data.SocialMedia?.Twitter,
                LinkedIn = result.Data.SocialMedia?.LinkedIn,
                Instagram = result.Data.SocialMedia?.Instagram,
                YouTube = result.Data.SocialMedia?.YouTube
            };

            return View(viewModel);
        }

        // POST: Admin/CompanyInfo/UpdateBasic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBasic(CompanyInfoEditViewModel model)
        {
            if (!ValidateModel(model))
            {
                return View("Index", model);
            }

        

            var command = new UpdateCompanyInfoBasicCommand
            {
                CompanyName = model.CompanyName,
                About = model.About,
                Mission = model.Mission,
                Vision = model.Vision,
                Slogan = model.Slogan,
                CopyrightText = model.CopyrightText,
                LogoPath = model.LogoPath,
                LogoLightPath = model.LogoLightPath
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Company information updated successfully",
                nameof(Index));
        }

        // POST: Admin/CompanyInfo/UpdateContact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateContact(CompanyInfoEditViewModel model)
        {
            var command = new UpdateCompanyInfoContactCommand
            {
                SupportPhone = model.SupportPhone,
                SalesPhone = model.SalesPhone,
                Email = model.Email,
                SupportEmail = model.SupportEmail,
                SalesEmail = model.SalesEmail
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Contact information updated successfully",
                nameof(Index));
        }

        // POST: Admin/CompanyInfo/UpdateAddress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAddress(CompanyInfoEditViewModel model)
        {
            var command = new UpdateCompanyInfoAddressCommand
            {
                Street = model.Street,
                Suite = model.Suite,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Address updated successfully",
                nameof(Index));
        }

        // POST: Admin/CompanyInfo/UpdateSocialMedia
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSocialMedia(CompanyInfoEditViewModel model)
        {
            var command = new UpdateCompanyInfoSocialMediaCommand
            {
                Facebook = model.Facebook,
                Twitter = model.Twitter,
                LinkedIn = model.LinkedIn,
                Instagram = model.Instagram,
                YouTube = model.YouTube
            };

            var result = await Mediator.Send(command);

            return HandleResult(result,
                "Social media links updated successfully",
                nameof(Index));
        }
    }
}