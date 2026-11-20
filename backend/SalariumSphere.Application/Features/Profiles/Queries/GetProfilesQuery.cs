using MediatR;
using SalariumSphere.Application.Features.Profiles.Common;

namespace SalariumSphere.Application.Features.Profiles.Queries;

public class GetProfilesQuery : IRequest<List<ProfileDto>>
{
    public bool IncludeInactive { get; set; } = false;
}