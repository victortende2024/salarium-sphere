using SalariumSphere.Domain.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Domain.Entities;

public class PayrollProfile : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal BaseSalaryAmount { get; set; }
    public CurrencyCode BaseSalaryCurrency { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<ProfileBenefit> DefaultBenefits { get; set; } = new List<ProfileBenefit>();
}