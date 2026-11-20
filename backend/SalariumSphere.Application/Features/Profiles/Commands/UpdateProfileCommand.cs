using MediatR;
using SalariumSphere.Application.Features.Profiles.Common;
using SalariumSphere.Domain.Enums;

namespace SalariumSphere.Application.Features.Profiles.Commands;

public class UpdateProfileCommand : IRequest<ProfileDto>
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal BaseSalaryAmount { get; set; }
    public CurrencyCode BaseSalaryCurrency { get; set; }
    public List<ProfileBenefitInputDto> DefaultBenefits { get; set; } = new();
}