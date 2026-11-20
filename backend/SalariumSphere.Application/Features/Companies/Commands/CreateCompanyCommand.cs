using MediatR;
using SalariumSphere.Application.Features.Companies.Common;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class CreateCompanyCommand : IRequest<CompanyDto>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid? ParentCompanyId { get; set; }
}