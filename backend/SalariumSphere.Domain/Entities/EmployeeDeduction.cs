using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Domain.Entities;

public class EmployeeDeduction : BaseEntity
{
    public required Guid EmployeeId { get; set; }
    public required string DeductionName { get; set; }
    public string? Description { get; set; }

    // Calculation Type: Fixed, Percentage, Tiered
    public DeductionCalculationType CalculationType { get; set; } = DeductionCalculationType.Fixed;

    // For Fixed Amount
    public decimal? FixedAmount { get; set; }
    public CurrencyCode? FixedAmountCurrency { get; set; }

    // For Percentage
    public decimal? PercentageValue { get; set; } // e.g., 20 means 20%
    public decimal? PercentageOfGross { get; set; } // e.g., 20 means 20% of gross

    // For Tiered/Progressive
    public string? TieredConfigJson { get; set; } // JSON array of tiers

    // Optional: Cap (maximum amount)
    public decimal? CapAmount { get; set; }
    public CurrencyCode? CapCurrency { get; set; }

    // Optional: Minimum threshold
    public decimal? ThresholdAmount { get; set; }
    public CurrencyCode? ThresholdCurrency { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public Employee? Employee { get; set; }
}