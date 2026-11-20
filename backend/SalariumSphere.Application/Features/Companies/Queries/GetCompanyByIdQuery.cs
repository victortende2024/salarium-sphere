using MediatR;
using SalariumSphere.Application.Features.Companies.Common;

namespace SalariumSphere.Application.Features.Companies.Queries;

public class GetCompanyByIdQuery : IRequest<CompanyDto>
{
    public required Guid Id { get; set; }
}