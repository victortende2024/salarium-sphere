using SalariumSphere.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class Department : BaseEntity
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required Guid CompanyId { get; set; }

        // Navigation property
        public Company? Company { get; set; }
    }
}
