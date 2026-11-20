using SalariumSphere.Application.Common;

namespace SalariumSphere.Application.Interfaces;

public interface IPayslipService
{
    Task<PayslipDto> GetPayslipAsync(Guid employeeId, Guid payrollRunId);
    Task<List<PayslipDto>> GetPayslipsForEmployeeAsync(Guid employeeId, int limit = 12);
    Task<List<PayslipDto>> GetPayslipsForCompanyAsync(Guid companyId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<byte[]> GeneratePayslipPdfAsync(Guid employeeId, Guid payrollRunId);
}