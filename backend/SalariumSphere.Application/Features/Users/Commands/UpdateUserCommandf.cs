using MediatR;
using SalariumSphere.Application.Features.Users.Common;

namespace SalariumSphere.Application.Features.Users.Commands;

public class UpdateUserCommand : IRequest<UserDto>
{
    public required Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool IsActive { get; set; }
}