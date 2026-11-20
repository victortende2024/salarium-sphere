using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Infrastructure.MasterData;
using SalariumSphere.Infrastructure.Persistence;
using SalariumSphere.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register the Master DbContext (uses a fixed connection string from appsettings)
            services.AddDbContext<MasterDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("MasterConnection")));
            

            services.AddScoped<ITenantResolver, TenantResolver>();
                        services.AddHttpContextAccessor();
                        services.AddScoped<DbContextFactory>();
                        services.AddScoped<IApplicationDbContext>(sp =>
                        {
                            var factory = sp.GetRequiredService<DbContextFactory>();
                            var dbContext = factory.CreateDbContextAsync().GetAwaiter().GetResult(); // synchronous for DI registration
                            return new ApplicationDbContextBridge(dbContext);
                        });
            services.AddScoped<IRulesEngineService, RulesEngineService>();
            services.AddScoped<IPayrollRunService, PayrollRunService>();
            services.AddScoped<IPayslipService, PayslipService>();
            return services;
        }
    }
}
