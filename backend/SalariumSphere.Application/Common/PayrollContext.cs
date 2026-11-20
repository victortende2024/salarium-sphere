using SalariumSphere.Domain.Entities;

public class PayrollContext
{
    public Employee Employee { get; set; } = null!;
    public PayrollProfile Profile { get; set; } = null!;
    public List<EmployeeCustomBenefit> CustomBenefits { get; set; } = new();
    public List<EmployeeSalaryStructure> SalaryStructures { get; set; } = new();
    public List<EmployeeDeduction> EmployeeDeductions { get; set; } = new();
    public List<PayrollTaxRule> TaxRules { get; set; } = new();
    public decimal GrossPay { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
}