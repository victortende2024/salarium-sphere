using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class PayrollRunDetail : BaseEntity
    {
        public required Guid PayrollRunId { get; set; }
        public required Guid EmployeeId { get; set; }
        public CurrencyCode Currency { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public string? LineItemsJson { get; set; } // JSON serialized list of PayrollLineItem
        public string? ErrorMessage { get; set; } // If an employee fails, log the error

        // Navigation
        public PayrollRun? PayrollRun { get; set; }
        public Employee? Employee { get; set; }
    }
}
