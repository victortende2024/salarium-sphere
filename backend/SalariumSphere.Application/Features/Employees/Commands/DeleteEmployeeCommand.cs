using MediatR;

namespace SalariumSphere.Application.Features.Employees.Commands;

public class DeleteEmployeeCommand : IRequest
{
    public required Guid Id { get; set; }
}