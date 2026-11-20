using SalariumSphere.Domain.Common;

namespace SalariumSphere.Domain.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role { get; set; }

    //  Foreign key – nullable
    public Guid? EmployeeId { get; set; }

    //  Navigation property
    public Employee? Employee { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
}