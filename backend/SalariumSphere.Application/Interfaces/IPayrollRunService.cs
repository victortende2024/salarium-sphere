using SalariumSphere.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Interfaces
{
    public interface IPayrollRunService
    {
        Task<PayrollRunSummary> RunPayrollAsync(PayrollRunRequest request);
    }
}
