using MediatR;
using SalariumSphere.Application.Features.Users.Common;

namespace SalariumSphere.Application.Features.Users.Queries;

public class GetUsersQuery : IRequest<List<UserDto>> { }