using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Companies.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Companies.Queries;

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public GetCompanyByIdQueryHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var company = await _context.Set<Company>()
            .Include(c => c.Departments)
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenant.Id, cancellationToken);
        if (company == null)
            throw new Exception($"Company with ID {request.Id} not found.");

        var allCompanies = await _context.Set<Company>()
            .Where(c => c.TenantId == tenant.Id)
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        return GetCompaniesQueryHandler.MapToDto(company, allCompanies);
    }
}