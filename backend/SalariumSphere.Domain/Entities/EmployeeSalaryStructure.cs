using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class EmployeeSalaryStructure : BaseEntity
    {
        public required Guid EmployeeId { get; set; }
        public CurrencyCode Currency { get; set; }
        public decimal Amount { get; set; }
        public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; } // Null means currently active

        // Navigation
        public Employee? Employee { get; set; }
    }
}
