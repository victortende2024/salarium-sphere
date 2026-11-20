using Microsoft.EntityFrameworkCore;
using SalariumSphere.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<T> Set<T>() where T : BaseEntity;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
