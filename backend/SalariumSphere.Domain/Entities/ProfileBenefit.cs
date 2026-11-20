using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Domain.Entities;

public class ProfileBenefit : BaseEntity
{
    public required Guid ProfileId { get; set; }
    public required string BenefitName { get; set; }
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public bool IsRecurring { get; set; } = true;

    // Navigation
    public PayrollProfile? Profile { get; set; }
}