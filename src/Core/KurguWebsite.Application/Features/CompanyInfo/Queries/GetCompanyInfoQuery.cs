// src/Core/KurguWebsite.Application/Features/CompanyInfo/Queries/GetCompanyInfoQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CompanyInfo;
using KurguWebsite.Domain.Specifications;
using MediatR;

namespace KurguWebsite.Application.Features.CompanyInfo.Queries
{
    public class GetCompanyInfoQuery : IRequest<Result<CompanyInfoDto>> { }

    public class GetCompanyInfoQueryHandler
        : IRequestHandler<GetCompanyInfoQuery, Result<CompanyInfoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public GetCompanyInfoQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<Result<CompanyInfoDto>> Handle(
            GetCompanyInfoQuery request,
            CancellationToken ct)
        {
            var cachedInfo = await _cacheService.GetAsync<CompanyInfoDto>(CacheKeys.CompanyInfo);
            if (cachedInfo != null)
            {
                return Result<CompanyInfoDto>.Success(cachedInfo);
            }

            var spec = new ActiveCompanyInfoSpecification();
            var companyInfoList = await _unitOfWork.CompanyInfo.ListAsync(spec, ct);
            var companyInfo = companyInfoList.FirstOrDefault();

            if (companyInfo == null)
            {
                return Result<CompanyInfoDto>.Failure(
                    "Company info not found.",
                    ErrorCodes.EntityNotFound);
            }

            var mappedInfo = _mapper.Map<CompanyInfoDto>(companyInfo);
            await _cacheService.SetAsync(CacheKeys.CompanyInfo, mappedInfo, TimeSpan.FromHours(1));

            return Result<CompanyInfoDto>.Success(mappedInfo);
        }
    }
}