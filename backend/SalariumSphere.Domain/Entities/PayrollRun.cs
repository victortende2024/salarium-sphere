using SalariumSphere.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class PayrollRun : BaseEntity
    {
        public required Guid CompanyId { get; set; }
        public DateTime PayPeriodStart { get; set; }
        public DateTime PayPeriodEnd { get; set; }
        public string Status { get; set; } = "Processing"; // Processing, Completed, Failed
        public int TotalEmployees { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public DateTime? CompletedAtUtc { get; set; }

        // Navigation
        public Company? Company { get; set; }
        public List<PayrollRunDetail> Details { get; set; } = new();
    }
}
