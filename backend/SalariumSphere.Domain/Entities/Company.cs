using SalariumSphere.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Domain.Entities
{
    public class Company : BaseEntity
    {
        public required string Name { get; set; }
        public Guid? ParentCompanyId { get; set; }

        // Navigation properties
        public Company? ParentCompany { get; set; }
        public ICollection<Company> ChildCompanies { get; set; } = new List<Company>();
        public ICollection<Department> Departments { get; set; } = new List<Department>();

    }
}
