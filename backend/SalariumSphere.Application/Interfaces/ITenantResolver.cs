using SalariumSphere.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Interfaces
{
    public interface ITenantResolver
    {
        Task<Tenant> GetCurrentTenantAsync();
    }
}
