using AutoMapper;
using KurguWebsite.Application.Contracts.Contact;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.Features.ContactMessages.Commands;
using KurguWebsite.WebUI.UIModel.CaseStudy;
using KurguWebsite.WebUI.UIModel.Contact;
using KurguWebsite.WebUI.UIModel.Service;
using System.Collections.Generic;
using System.Linq;

namespace KurguWebsite.WebUI.Mappings
{
    public class WebUiMappingProfile : Profile
    {
        public WebUiMappingProfile()
        {
            // Root map
            CreateMap<ServiceDetailDto, ServiceDetailViewModel>()
                // ensure non-null lists so the view doesn't null-ref
                .ForMember(d => d.Features,
                    o => o.MapFrom(s => (IEnumerable<ServiceFeatureDto>)(s.Features ?? Enumerable.Empty<ServiceFeatureDto>())))
                .ForMember(d => d.OtherServices,
                    o => o.MapFrom(s => (IEnumerable<ServiceDto>)(s.OtherServices ?? Enumerable.Empty<ServiceDto>())));

            // Child maps (these were missing)
            CreateMap<ServiceFeatureDto, ServiceFeatureVm>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Summary, o => o.MapFrom(s => s.Description))
                .ForMember(d => d.IconcClass, o => o.MapFrom(s => s.IconClass));

            CreateMap<ServiceDto, OtherServiceVm>()
                .ForMember(d => d.Title, o => o.MapFrom(s => s.Title))
                .ForMember(d => d.Slug, o => o.MapFrom(s => s.Slug));




            CreateMap<CaseStudyDto, CaseStudyDetailViewModel>();
            // Metrics
            CreateMap<CaseStudyMetricDto, CaseStudyMetricVm>();
            // Card/List
            CreateMap<CaseStudyDto, CaseStudyCardVm>();
            CreateMap<ContactFormViewModel, ContactMessageRequest>();

        }
    }
}
