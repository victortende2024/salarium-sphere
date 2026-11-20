using MediatR;
using SalariumSphere.Application.Features.Companies.Common;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class UpdateCompanyCommand : IRequest<CompanyDto>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public Guid? ParentCompanyId { get; set; }
    public bool IsActive { get; set; }
}