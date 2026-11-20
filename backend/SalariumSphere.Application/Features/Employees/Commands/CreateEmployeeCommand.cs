using MediatR;
using SalariumSphere.Application.Features.Employees.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Features.Employees.Commands;

public class CreateEmployeeCommand : IRequest<EmployeeDto>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? EmployeeCode { get; set; } // Auto-generate if not provided
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

public class SalaryStructureInputDto
{
    public CurrencyCode Currency { get; set; }
    public decimal Amount { get; set; }
    public DateTime EffectiveStartDate { get; set; }
    public DateTime? EffectiveEndDate { get; set; }
}

public class CustomBenefitInputDto
{
    public string BenefitName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public bool IsRecurring { get; set; } = true;
}