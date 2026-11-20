using SalariumSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Features.Employees.Common
{

    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string EmployeeCode { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankBranchCode { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid PrimaryProfileId { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<SalaryStructureDto> SalaryStructures { get; set; } = new();
        public List<CustomBenefitDto> CustomBenefits { get; set; } = new();
    }

    public class SalaryStructureDto
    {
        public Guid Id { get; set; }
        public CurrencyCode Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }

    public class CustomBenefitDto
    {
        public Guid Id { get; set; }
        public string BenefitName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; }
        public bool IsRecurring { get; set; }
    }
}
