using SalariumSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Common
{
    public class PayrollResult
    {
        public Guid EmployeeId { get; set; }
        public CurrencyCode Currency { get; set; }
        public decimal GrossPay { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal NetPay { get; set; }
        public List<PayrollLineItem> LineItems { get; set; } = new();
    }

    public class PayrollLineItem
    {
        public string Name { get; set; }
        public string Type { get; set; } // "Earning", "Deduction", "Benefit"
        public decimal Amount { get; set; }
        public string Source { get; set; } // "Profile", "CustomBenefit", "Rule"
    }
}
