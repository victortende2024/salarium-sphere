using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Profiles.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Profiles.Queries;

public class GetProfileByIdQueryHandler : IRequestHandler<GetProfileByIdQuery, ProfileDto>
{
    private readonly IApplicationDbContext _context;

    public GetProfileByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProfileDto> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.Set<PayrollProfile>()
            .Include(p => p.DefaultBenefits)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (profile == null)
            throw new Exception($"Profile with ID {request.Id} not found.");

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