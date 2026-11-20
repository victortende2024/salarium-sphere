using SalariumSphere.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class Employee : BaseEntity
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string EmployeeCode { get; set; } // Unique employee ID within the tenant

        public DateTime? JoinedDate { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankBranchCode { get; set; }

        // Foreign Keys
        public required Guid CompanyId { get; set; }
        public required Guid DepartmentId { get; set; }
        public required Guid PrimaryProfileId { get; set; }

        // Navigation properties
        public Company? Company { get; set; }
        public Department? Department { get; set; }
        public PayrollProfile? PrimaryProfile { get; set; }

        public ICollection<EmployeeSalaryStructure> EmployeeSalaryStructures { get; set; } = new List<EmployeeSalaryStructure>();
        public ICollection<EmployeeCustomBenefit> EmployeeCustomBenefits { get; set; } = new List<EmployeeCustomBenefit>();
    }
}
