using SalariumSphere.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class Tenant : BaseEntity, IAggregateRoot
    {
        public required string Name { get; set; }
        public required string Subdomain { get; set; }
        public required string ConnectionString { get; set; }
    }
}
