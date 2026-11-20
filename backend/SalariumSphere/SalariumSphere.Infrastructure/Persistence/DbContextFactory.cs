using SalariumSphere.Application.Interfaces;
using SalariumSphere.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Infrastructure.Persistence
{
    public class DbContextFactory
    {
        private readonly ITenantResolver _tenantResolver;

        public DbContextFactory(ITenantResolver tenantResolver)
        {
            _tenantResolver = tenantResolver;
        }

        public async Task<ApplicationDbContext> CreateDbContextAsync()
        {
            var tenant = await _tenantResolver.GetCurrentTenantAsync();
            // Optional: decrypt connection string here
            return new ApplicationDbContext(tenant.ConnectionString);
        }
    }
}
