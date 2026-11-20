using MediatR;
using SalariumSphere.Application.Features.Users.Common;

namespace SalariumSphere.Application.Features.Users.Commands;

public class CreateUserCommand : IRequest<UserDto>
{
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role { get; set; }
    public Guid? EmployeeId { get; set; }
    public string? Password { get; set; } // If not provided, auto-generate
}