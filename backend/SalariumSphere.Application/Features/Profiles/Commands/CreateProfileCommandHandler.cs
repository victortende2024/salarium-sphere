using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Profiles.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Profiles.Commands;

public class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, ProfileDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProfileCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProfileDto> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        // Validate unique name per tenant (you'll need to get current tenant)
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // Replace with actual tenant resolution
        var existing = await _context.Set<PayrollProfile>()
            .AnyAsync(p => p.Name == request.Name && p.TenantId == tenantId, cancellationToken);
        if (existing)
            throw new Exception("A profile with this name already exists.");

        var profile = new PayrollProfile
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            BaseSalaryAmount = request.BaseSalaryAmount,
            BaseSalaryCurrency = request.BaseSalaryCurrency,
            TenantId = tenantId,
            IsActive = true
        };

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
                TenantId = tenantId
            });
        }

        await _context.Set<PayrollProfile>().AddAsync(profile, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(profile);
    }

    private ProfileDto MapToDto(PayrollProfile profile) => new()
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