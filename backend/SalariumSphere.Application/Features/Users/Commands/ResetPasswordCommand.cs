using MediatR;

namespace SalariumSphere.Application.Features.Users.Commands;

public class ResetPasswordCommand : IRequest
{
    public required Guid Id { get; set; }
    public required string NewPassword { get; set; }
}