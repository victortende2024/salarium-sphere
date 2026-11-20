using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Profiles.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Profiles.Queries;

public class GetProfilesQueryHandler : IRequestHandler<GetProfilesQuery, List<ProfileDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProfilesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ProfileDto>> Handle(GetProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<PayrollProfile>()
            .Include(p => p.DefaultBenefits)
            .AsQueryable();

        if (!request.IncludeInactive)
            query = query.Where(p => p.IsActive);

        var profiles = await query.OrderBy(p => p.Name).ToListAsync(cancellationToken);
        return profiles.Select(MapToDto).ToList();
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