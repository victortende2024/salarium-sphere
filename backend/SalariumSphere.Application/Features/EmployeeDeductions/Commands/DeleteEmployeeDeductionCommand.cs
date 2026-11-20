using MediatR;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Commands;

public class DeleteEmployeeDeductionCommand : IRequest
{
    public required Guid Id { get; set; }
}