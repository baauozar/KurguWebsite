using AutoMapper;
using KurguWebsite.Application.DTOs;
using KurguWebsite.Application.DTOs.Service;
using KurguWebsite.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Service Mappings
            CreateMap<Service, ServiceDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

            CreateMap<Service, ServiceDetailDto>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.Features))
                .ForMember(dest => dest.OtherServices, opt => opt.Ignore())
                .ForMember(dest => dest.RelatedCaseStudies, opt => opt.Ignore());

            CreateMap<ServiceFeature, ServiceFeatureDto>();

            // Case Study Mappings
            CreateMap<CaseStudy, CaseStudyDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.Title : null));

            CreateMap<CaseStudy, CaseStudyDetailDto>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.Title : null));

            // Testimonial Mappings
            CreateMap<Testimonial, TestimonialDto>();

            // Partner Mappings
            CreateMap<Partner, PartnerDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

            // Page Mappings
            CreateMap<Page, PageDto>()
                .ForMember(dest => dest.PageType, opt => opt.MapFrom(src => src.PageType.ToString()));

            CreateMap<ProcessStep, ProcessStepDto>();

            // Contact Message Mappings
            CreateMap<ContactMessage, ContactMessageDto>();

            // Company Info Mappings
            CreateMap<CompanyInfo, CompanyInfoDto>()
                .ForMember(dest => dest.SupportPhone, opt => opt.MapFrom(src => src.ContactInformation.SupportPhone))
                .ForMember(dest => dest.SalesPhone, opt => opt.MapFrom(src => src.ContactInformation.SalesPhone))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.ContactInformation.Email))
                .ForMember(dest => dest.SupportEmail, opt => opt.MapFrom(src => src.ContactInformation.SupportEmail))
                .ForMember(dest => dest.SalesEmail, opt => opt.MapFrom(src => src.ContactInformation.SalesEmail))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.OfficeAddress.Street))
                .ForMember(dest => dest.Suite, opt => opt.MapFrom(src => src.OfficeAddress.Suite))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.OfficeAddress.City))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.OfficeAddress.State))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.OfficeAddress.PostalCode))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.OfficeAddress.Country))
                .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => src.OfficeAddress.GetFullAddress()))
                .ForMember(dest => dest.Facebook, opt => opt.MapFrom(src => src.SocialMedia != null ? src.SocialMedia.Facebook : null))
                .ForMember(dest => dest.Twitter, opt => opt.MapFrom(src => src.SocialMedia != null ? src.SocialMedia.Twitter : null))
                .ForMember(dest => dest.LinkedIn, opt => opt.MapFrom(src => src.SocialMedia != null ? src.SocialMedia.LinkedIn : null))
                .ForMember(dest => dest.Instagram, opt => opt.MapFrom(src => src.SocialMedia != null ? src.SocialMedia.Instagram : null))
                .ForMember(dest => dest.YouTube, opt => opt.MapFrom(src => src.SocialMedia != null ? src.SocialMedia.YouTube : null));
        }
    }
}

