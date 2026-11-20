using MediatR;
using SalariumSphere.Application.Features.Employees.Common;

namespace SalariumSphere.Application.Features.Employees.Queries;

public class GetEmployeeByIdQuery : IRequest<EmployeeDto>
{
    public Guid Id { get; set; }
}