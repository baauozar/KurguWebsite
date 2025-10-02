using AutoMapper;

// DTOs
using KurguWebsite.Application.DTOs.Audit;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.Testimonial;

// Commands
using KurguWebsite.Application.Features.CaseStudies.Commands;

// Domain
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.ValueObjects;

namespace KurguWebsite.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // ---------------------------
            // Service & ServiceFeature
            // ---------------------------
            CreateMap<Service, ServiceDto>();

            CreateMap<Service, ServiceDetailDto>()
                .ForMember(d => d.Features, o => o.MapFrom(s => s.Features));

            CreateMap<ServiceFeature, ServiceFeatureDto>();
            CreateMap<ServiceFeature, CreateServiceFeatureDto>().ReverseMap();

            CreateMap<CreateServiceDto, Service>();
            CreateMap<UpdateServiceDto, Service>();

            // ---------------------------
            // CaseStudy & CaseStudyMetric
            // ---------------------------
            CreateMap<CaseStudy, CaseStudyDto>()
                .ForMember(d => d.ServiceName, o => o.MapFrom(s => s.Service != null ? s.Service.Title : null));

            CreateMap<CreateCaseStudyDto, CaseStudy>();
            CreateMap<UpdateCaseStudyDto, CaseStudy>();

            // Entity -> DTO
            CreateMap<CaseStudyMetric, CaseStudyMetricDto>();
            // If you expose DisplayOrder in DTO, add: .ForMember(d => d.DisplayOrder, o => o.MapFrom(s => s.DisplayOrder));

            // Command -> Entity (be explicit; handler sets relations/order)
            CreateMap<CreateCaseStudyMetricCommand, CaseStudyMetric>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CaseStudy, o => o.Ignore());
            // If DisplayOrder is set in handler, also ignore here:
            // .ForMember(d => d.DisplayOrder, o => o.Ignore());

            // ---------------------------
            // Testimonial
            // ---------------------------
            CreateMap<Testimonial, TestimonialDto>();
            CreateMap<CreateTestimonialDto, Testimonial>();
            CreateMap<UpdateTestimonialDto, Testimonial>();

            // ---------------------------
            // Partner
            // ---------------------------
            CreateMap<Partner, PartnerDto>();
            CreateMap<CreatePartnerDto, Partner>();
            CreateMap<UpdatePartnerDto, Partner>();

            // ---------------------------
            // Page
            // ---------------------------
            CreateMap<Page, PageDto>();
            CreateMap<CreatePageDto, Page>();
            CreateMap<UpdatePageDto, Page>();

            // ---------------------------
            // ProcessStep
            // ---------------------------
            CreateMap<ProcessStep, ProcessStepDto>();
            CreateMap<CreateProcessStepDto, ProcessStep>();
            CreateMap<UpdateProcessStepDto, ProcessStep>();

            // ---------------------------
            // CompanyInfo & value objects
            // ---------------------------
            CreateMap<CompanyInfo, CompanyInfoDto>()
                .ForMember(d => d.ContactInformation, o => o.MapFrom(s => s.ContactInformation))
                .ForMember(d => d.OfficeAddress, o => o.MapFrom(s => s.OfficeAddress))
                .ForMember(d => d.SocialMedia, o => o.MapFrom(s => s.SocialMedia));

            CreateMap<ContactInfo, ContactInfoDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<SocialMediaLinks, SocialMediaDto>();

            // ---------------------------
            // ContactMessage
            // ---------------------------
            CreateMap<ContactMessage, ContactMessageDto>();
            CreateMap<CreateContactMessageDto, ContactMessage>();

            // ---------------------------
            // Audit
            // ---------------------------
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(d => d.UserName, o => o.MapFrom(s => s.UserName));
        }
    }
}
