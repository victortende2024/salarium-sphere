using SalariumSphere.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class PayrollRule : BaseEntity
    {
        public required string WorkflowName { get; set; } // e.g., "SalaryCalculation", "TaxDeduction"
        public required string RuleName { get; set; } // e.g., "DriverFuelAllowance"
        public required string RuleJson { get; set; } // The JSON representation of the Rule
        public int Priority { get; set; } // Order of execution
    }
}
