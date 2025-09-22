using AutoMapper;
using KurguWebsite.Application.DTOs.CaseStudy;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Application.DTOs.Contact;
using KurguWebsite.Application.DTOs.Page;
using KurguWebsite.Application.DTOs.Partner;
using KurguWebsite.Application.DTOs.ProcessStep;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Application.DTOs.Testimonial;
using KurguWebsite.Domain.Entities;
using KurguWebsite.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Mappings
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Service mappings
            CreateMap<Service, ServiceDto>();
            CreateMap<Service, ServiceDetailDto>()
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features));
            CreateMap<ServiceFeature, ServiceFeatureDto>();
            CreateMap<CreateServiceDto, Service>();
            CreateMap<UpdateServiceDto, Service>();

            // CaseStudy mappings
            CreateMap<CaseStudy, CaseStudyDto>()
                .ForMember(dest => dest.ServiceName,
                    opt => opt.MapFrom(src => src.Service != null ? src.Service.Title : null));
            CreateMap<CreateCaseStudyDto, CaseStudy>();
            CreateMap<UpdateCaseStudyDto, CaseStudy>();

            // Testimonial mappings
            CreateMap<Testimonial, TestimonialDto>();
            CreateMap<CreateTestimonialDto, Testimonial>();
            CreateMap<UpdateTestimonialDto, Testimonial>();

            // Partner mappings
            CreateMap<Partner, PartnerDto>();
            CreateMap<CreatePartnerDto, Partner>();
            CreateMap<UpdatePartnerDto, Partner>();

            // Page mappings
            CreateMap<Page, PageDto>();
            CreateMap<CreatePageDto, Page>();
            CreateMap<UpdatePageDto, Page>();

            // ProcessStep mappings
            CreateMap<ProcessStep,ProcessStepDto>();
            CreateMap<CreateProcessStepDto, ProcessStep>();
            CreateMap<UpdateProcessStepDto, ProcessStep>();

            // CompanyInfo mappings
            CreateMap<CompanyInfo, CompanyInfoDto>()
                .ForMember(dest => dest.ContactInformation,
                    opt => opt.MapFrom(src => src.ContactInformation))
                .ForMember(dest => dest.OfficeAddress,
                    opt => opt.MapFrom(src => src.OfficeAddress))
                .ForMember(dest => dest.SocialMedia,
                    opt => opt.MapFrom(src => src.SocialMedia));

            CreateMap<ContactInfo, ContactInfoDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<SocialMediaLinks, SocialMediaDto>();

            // ContactMessage mappings
            CreateMap<ContactMessage, ContactMessageDto>();
            CreateMap<CreateContactMessageDto, ContactMessage>();
        }
    }
}