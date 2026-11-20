using SalariumSphere.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Interfaces
{
    public interface IRulesEngineService
    {
        // Evaluates a specific workflow (e.g., "BenefitCalculation") for a given PayrollContext
        Task<PayrollResult> ExecuteWorkflowAsync(string workflowName, PayrollContext context);
    }
}
