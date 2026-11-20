using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Domain.Entities;

public class PayrollTaxRule : BaseEntity
{
    public required string Name { get; set; } // e.g., "PAYE", "Social Security"
    public string? Description { get; set; }
    public string? CountryCode { get; set; } // Optional: country-specific
    public bool IsMandatory { get; set; } = true;

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

    public int Priority { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum DeductionCalculationType
{
    Fixed = 0,
    Percentage = 1,
    Tiered = 2,
}