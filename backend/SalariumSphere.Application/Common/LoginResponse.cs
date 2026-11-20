using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalariumSphere.Application.Common
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required Guid UserId { get; set; }
        public Guid? EmployeeId { get; set; } = Guid.Empty;
        public required Guid TenantId { get; set; }
    }
}
