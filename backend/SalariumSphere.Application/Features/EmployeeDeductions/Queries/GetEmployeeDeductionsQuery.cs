using MediatR;
using SalariumSphere.Application.Features.EmployeeDeductions.Common;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Queries;

public class GetEmployeeDeductionsQuery : IRequest<List<EmployeeDeductionDto>>
{
    public required Guid EmployeeId { get; set; }
}