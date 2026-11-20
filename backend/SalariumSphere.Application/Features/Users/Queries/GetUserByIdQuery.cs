using MediatR;
using SalariumSphere.Application.Features.Users.Common;

namespace SalariumSphere.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public required Guid Id { get; set; }
}