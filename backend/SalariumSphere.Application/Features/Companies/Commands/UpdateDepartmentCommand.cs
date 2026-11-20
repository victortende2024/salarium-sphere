using MediatR;
using SalariumSphere.Application.Features.Companies.Common;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class UpdateDepartmentCommand : IRequest<DepartmentDto>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required Guid CompanyId { get; set; }
    public bool IsActive { get; set; }
}