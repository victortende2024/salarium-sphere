using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Common;

public class PayslipDto
{
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public string PayPeriod { get; set; } = string.Empty;
    public DateTime PayPeriodStart { get; set; }
    public DateTime PayPeriodEnd { get; set; }
    public CurrencyCode Currency { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
    public List<PayslipLineItemDto> LineItems { get; set; } = new();
    
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class PayslipLineItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Earning, Tax, Deduction
    public decimal Amount { get; set; }
    public string Source { get; set; } = string.Empty;
}