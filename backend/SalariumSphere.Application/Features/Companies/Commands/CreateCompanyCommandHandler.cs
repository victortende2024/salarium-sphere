using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Companies.Common;
using SalariumSphere.Application.Features.Companies.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Commands;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public CreateCompanyCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        // Validate parent company exists and belongs to tenant
        if (request.ParentCompanyId.HasValue)
        {
            var parent = await _context.Set<Company>()
                .FirstOrDefaultAsync(c => c.Id == request.ParentCompanyId.Value && c.TenantId == tenant.Id, cancellationToken);
            if (parent == null)
                throw new Exception("Parent company not found or does not belong to this tenant.");

            if (request.ParentCompanyId.Value == request.Id)
                throw new Exception("A company cannot be its own parent.");
        }

     

        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ParentCompanyId = request.ParentCompanyId,
            TenantId = tenant.Id,
            IsActive = true
   
        };

        

        await _context.Set<Company>().AddAsync(company, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var allCompanies = await _context.Set<Company>()
            .Where(c => c.TenantId == tenant.Id)
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return GetCompaniesQueryHandler.MapToDto(company, allCompanies);
    }
}