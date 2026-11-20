using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class EmployeeCustomBenefit : BaseEntity
    {
        public required Guid EmployeeId { get; set; }
        public required string BenefitName { get; set; } // e.g., "Fuel Allowance"
        public decimal Amount { get; set; }
        public CurrencyCode Currency { get; set; }
        public bool IsRecurring { get; set; } = true; // Monthly or one-off
        public string? Description { get; set; }

        // Navigation
       
        public Employee? Employee { get; set; }
    }
}
