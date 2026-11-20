using MediatR;
using SalariumSphere.Application.Features.Employees.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Features.Employees.Queries
{
    public class GetEmployeesQuery : IRequest<List<EmployeeDto>>
    {
        public Guid? CompanyId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProfileId { get; set; }
        public string? SearchTerm { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }
}
