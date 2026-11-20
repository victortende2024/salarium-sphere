using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Common
{
    public class PayrollRunSummary
    {
        public Guid PayrollRunId { get; set; }
        public int TotalEmployees { get; set; }
        public int SuccessfulEmployees { get; set; }
        public int FailedEmployees { get; set; }
        public decimal TotalGrossPay { get; set; }
        public decimal TotalNetPay { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}
