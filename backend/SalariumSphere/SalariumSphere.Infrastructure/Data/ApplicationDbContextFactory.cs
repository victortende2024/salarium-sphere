using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Infrastructure.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Use the connection string passed via the --connection command-line argument.
          // If none is provided, fall back to a local default for safety.
            var connectionString = args.Length > 0
                ? args[0]
                : "Server=VICKIETENDEPC\\MSSQLSERVER01;Database=Salarium_Acme;Trusted_Connection=True;TrustServerCertificate=true;";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(connectionString);
        }
    }
}
