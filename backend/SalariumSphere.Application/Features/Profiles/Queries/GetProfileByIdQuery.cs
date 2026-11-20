using MediatR;
using SalariumSphere.Application.Features.Profiles.Common;

namespace SalariumSphere.Application.Features.Profiles.Queries;

public class GetProfileByIdQuery : IRequest<ProfileDto>
{
    public required Guid Id { get; set; }
}