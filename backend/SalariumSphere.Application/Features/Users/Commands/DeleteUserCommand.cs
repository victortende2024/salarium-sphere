using MediatR;

namespace SalariumSphere.Application.Features.Users.Commands;

public class DeleteUserCommand : IRequest
{
    public required Guid Id { get; set; }
}