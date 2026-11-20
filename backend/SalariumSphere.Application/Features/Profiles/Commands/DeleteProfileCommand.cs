using MediatR;

namespace SalariumSphere.Application.Features.Profiles.Commands;

public class DeleteProfileCommand : IRequest
{
    public required Guid Id { get; set; }
}