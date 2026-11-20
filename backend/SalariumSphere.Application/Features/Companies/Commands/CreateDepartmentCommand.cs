using MediatR;
using SalariumSphere.Application.Features.Companies.Common;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class CreateDepartmentCommand : IRequest<DepartmentDto>
{
    public required string Name { get; set; }
    public required Guid CompanyId { get; set; }
}