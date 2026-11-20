using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Common;
using SalariumSphere.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Infrastructure.Persistence
{
    public class ApplicationDbContextBridge : IApplicationDbContext
    {
        private readonly ApplicationDbContext _dbContext;

        public ApplicationDbContextBridge(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DbSet<T> Set<T>() where T : BaseEntity => _dbContext.Set<T>();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _dbContext.SaveChangesAsync(cancellationToken);
    }

}
