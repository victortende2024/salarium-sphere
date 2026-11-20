using MediatR;
using SalariumSphere.Application.Features.Employees.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Features.Employees.Commands;

public class UpdateEmployeeCommand : IRequest<EmployeeDto>
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? EmployeeCode { get; set; }
    public DateTime? JoinedDate { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankBranchCode { get; set; }
    public required Guid CompanyId { get; set; }
    public required Guid DepartmentId { get; set; }
    public required Guid PrimaryProfileId { get; set; }
    public List<SalaryStructureInputDto> SalaryStructures { get; set; } = new();
    public List<CustomBenefitInputDto> CustomBenefits { get; set; } = new();
}