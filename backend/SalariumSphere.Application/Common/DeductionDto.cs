using SalariumSphere.Domain.Entities;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Common;

public class DeductionDto
{
    public string Name { get; set; } = string.Empty;
    public DeductionCalculationType CalculationType { get; set; }
    public decimal? FixedAmount { get; set; }
    public CurrencyCode? FixedAmountCurrency { get; set; }
    public decimal? PercentageValue { get; set; }
    public string? TieredConfigJson { get; set; }
    public decimal? CapAmount { get; set; }
    public CurrencyCode? CapCurrency { get; set; }
    public decimal? ThresholdAmount { get; set; }
    public CurrencyCode? ThresholdCurrency { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TaxRuleDto : DeductionDto
{
    public Guid Id { get; set; }
    public string? CountryCode { get; set; }
    public bool IsMandatory { get; set; }
    public int Priority { get; set; }
}

//public class EmployeeDeductionDto : DeductionDto
//{
//    public Guid Id { get; set; }
//    public Guid EmployeeId { get; set; }
//}