// src/Core/KurguWebsite.Application/Features/CompanyInfo/Queries/GetCompanyInfoByIdQuery.cs
using AutoMapper;
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using KurguWebsite.Application.DTOs.CompanyInfo;
using MediatR;

namespace KurguWebsite.Application.Features.CompanyInfo.Queries
{
    public class GetCompanyInfoByIdQuery : IRequest<Result<CompanyInfoDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetCompanyInfoByIdQueryHandler
        : IRequestHandler<GetCompanyInfoByIdQuery, Result<CompanyInfoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCompanyInfoByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<CompanyInfoDto>> Handle(
            GetCompanyInfoByIdQuery request,
            CancellationToken ct)
        {
            var companyInfo = await _unitOfWork.CompanyInfo.GetByIdAsync(request.Id, ct);
            if (companyInfo == null)
            {
                return Result<CompanyInfoDto>.Failure(
                    "Company info not found.",
                    ErrorCodes.EntityNotFound);
            }

            return Result<CompanyInfoDto>.Success(_mapper.Map<CompanyInfoDto>(companyInfo));
        }
    }
}