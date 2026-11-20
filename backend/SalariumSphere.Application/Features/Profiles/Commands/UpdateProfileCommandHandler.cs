using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Profiles.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Profiles.Commands;

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        // Fetch the existing profile with its benefits
        var profile = await _context.Set<PayrollProfile>()
            .Include(p => p.DefaultBenefits)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile == null)
            throw new Exception($"Profile with ID {request.Id} was not found.");

        // Update basic properties
        profile.Name = request.Name;
        profile.Description = request.Description;
        profile.BaseSalaryAmount = request.BaseSalaryAmount;
        profile.BaseSalaryCurrency = request.BaseSalaryCurrency;
        profile.UpdatedAtUtc = DateTime.UtcNow;

        // Replace benefits: remove old ones and add new ones
        _context.Set<ProfileBenefit>().RemoveRange(profile.DefaultBenefits);
        profile.DefaultBenefits.Clear();

        foreach (var benefitDto in request.DefaultBenefits)
        {
            profile.DefaultBenefits.Add(new ProfileBenefit
            {
                Id = Guid.NewGuid(),
                ProfileId = profile.Id,
                BenefitName = benefitDto.BenefitName,
                Amount = benefitDto.Amount,
                Currency = benefitDto.Currency,
                IsRecurring = benefitDto.IsRecurring,
                TenantId = profile.TenantId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Re-fetch the updated profile to include benefits
        var updated = await _context.Set<PayrollProfile>()
            .Include(p => p.DefaultBenefits)
            .FirstAsync(p => p.Id == request.Id, cancellationToken);

        return MapToDto(updated);
    }

    private ProfileDto MapToDto(PayrollProfile profile)
    {
        return new ProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            Description = profile.Description,
            BaseSalaryAmount = profile.BaseSalaryAmount,
            BaseSalaryCurrency = profile.BaseSalaryCurrency,
            IsActive = profile.IsActive,
            DefaultBenefits = profile.DefaultBenefits?.Select(b => new ProfileBenefitDto
            {
                Id = b.Id,
                BenefitName = b.BenefitName,
                Amount = b.Amount,
                Currency = b.Currency,
                IsRecurring = b.IsRecurring
            }).ToList() ?? new()
        };
    }
}