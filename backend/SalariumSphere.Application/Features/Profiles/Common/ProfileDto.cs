using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Features.Profiles.Common;

public class ProfileDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal BaseSalaryAmount { get; set; }
    public CurrencyCode BaseSalaryCurrency { get; set; }
    public bool IsActive { get; set; }
    public List<ProfileBenefitDto> DefaultBenefits { get; set; } = new();
}

public class ProfileBenefitDto
{
    public Guid Id { get; set; }
    public string BenefitName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public bool IsRecurring { get; set; }
}