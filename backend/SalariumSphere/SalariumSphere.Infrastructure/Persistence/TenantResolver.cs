using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;
using SalariumSphere.Infrastructure.MasterData;
using System.Security.Claims;

namespace SalariumSphere.Infrastructure.Persistence;

public class TenantResolver : ITenantResolver
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MasterDbContext _masterDbContext;
    private Tenant _cachedTenant;

    public TenantResolver(IHttpContextAccessor httpContextAccessor, MasterDbContext masterDbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _masterDbContext = masterDbContext;
    }

    public async Task<Tenant> GetCurrentTenantAsync()
    {
        if (_cachedTenant != null)
            return _cachedTenant;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("No HTTP context available.");

        var tenantIdClaim = httpContext.User.FindFirst("tenant_id")?.Value;

        // If no tenant_id claim (e.g., during login), fallback to a default tenant.
        if (string.IsNullOrEmpty(tenantIdClaim))
        {
            // Use a known tenant – in development, we use the "acme" tenant.
            // For production, you should have a better strategy (e.g., subdomain-based resolution).
            var defaultTenant = await _masterDbContext.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == "acme" && t.IsActive);
            if (defaultTenant == null)
                throw new UnauthorizedAccessException("Default tenant not found.");
            _cachedTenant = defaultTenant;
            return _cachedTenant;
        }

        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            throw new UnauthorizedAccessException("Invalid tenant ID format.");

        var tenant = await _masterDbContext.Tenants
            .FirstOrDefaultAsync(t => t.Id == tenantId && t.IsActive);
        if (tenant == null)
            throw new UnauthorizedAccessException("Tenant not found or inactive.");

        _cachedTenant = tenant;
        return tenant;
    }
}