using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Companies.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Queries;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, List<CompanyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetCompaniesQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var companies = await _context.Set<Company>()
            .Include(c => c.Departments)
            .Where(c => c.TenantId == tenant.Id)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        companies = companies.GroupBy(c => c.Id).Select(g => g.First()).ToList();
        var allCompanies = companies.ToDictionary(c => c.Id);
        

        return companies.Select(c => MapToDto(c, allCompanies)).ToList();
    }

    public static CompanyDto MapToDto(Company c, Dictionary<Guid, Company> allCompanies)
    {
        return new CompanyDto
        {
            Id = c.Id,
            Name = c.Name,
            ParentCompanyId = c.ParentCompanyId,
            ParentCompanyName = c.ParentCompanyId.HasValue && allCompanies.ContainsKey(c.ParentCompanyId.Value)
                ? allCompanies[c.ParentCompanyId.Value].Name
                : null,
            IsActive = c.IsActive,
            Departments = c.Departments?.Where(d => d.IsActive).Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                CompanyId = d.CompanyId,
                IsActive = d.IsActive
            }).ToList() ?? new()
        };
    }
}