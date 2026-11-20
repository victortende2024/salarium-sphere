using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Companies.Common;
using SalariumSphere.Application.Features.Companies.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public UpdateCompanyCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var company = await _context.Set<Company>()
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenant.Id, cancellationToken);
        if (company == null)
            throw new Exception($"Company with ID {request.Id} not found.");

        company.Name = request.Name;
        company.ParentCompanyId = request.ParentCompanyId;
        company.IsActive = request.IsActive;
        company.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var allCompanies = await _context.Set<Company>()
            .Where(c => c.TenantId == tenant.Id)
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return GetCompaniesQueryHandler.MapToDto(company, allCompanies);
    }
}