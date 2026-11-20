using MediatR;
using SalariumSphere.Application.Features.Profiles.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Features.Profiles.Commands;

public class CreateProfileCommand : IRequest<ProfileDto>
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal BaseSalaryAmount { get; set; }
    public CurrencyCode BaseSalaryCurrency { get; set; }
    public List<ProfileBenefitInputDto> DefaultBenefits { get; set; } = new();
}

public class ProfileBenefitInputDto
{
    public string BenefitName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public CurrencyCode Currency { get; set; }
    public bool IsRecurring { get; set; } = true;
}