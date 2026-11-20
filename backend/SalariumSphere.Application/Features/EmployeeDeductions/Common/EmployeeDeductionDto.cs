using SalariumSphere.Domain.Entities;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Features.EmployeeDeductions.Common;

public class EmployeeDeductionDto
{
    public Guid Id { get; set; }
    public required Guid EmployeeId { get; set; }
    public required string DeductionName { get; set; }
    public string? Description { get; set; }
    public DeductionCalculationType CalculationType { get; set; }
    public decimal? FixedAmount { get; set; }
    public CurrencyCode? FixedAmountCurrency { get; set; }
    public decimal? PercentageValue { get; set; }
    public string? TieredConfigJson { get; set; }
    public decimal? CapAmount { get; set; }
    public CurrencyCode? CapCurrency { get; set; }
    public decimal? ThresholdAmount { get; set; }
    public CurrencyCode? ThresholdCurrency { get; set; }
    public bool IsActive { get; set; }
}