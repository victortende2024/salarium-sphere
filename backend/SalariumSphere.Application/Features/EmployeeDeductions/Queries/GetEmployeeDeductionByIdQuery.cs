using MediatR;
using SalariumSphere.Application.Features.EmployeeDeductions.Common;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Queries;

public class GetEmployeeDeductionByIdQuery : IRequest<EmployeeDeductionDto>
{
    public required Guid Id { get; set; }
}