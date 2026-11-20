using MediatR;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class DeleteDepartmentCommand : IRequest
{
    public required Guid Id { get; set; }
}
