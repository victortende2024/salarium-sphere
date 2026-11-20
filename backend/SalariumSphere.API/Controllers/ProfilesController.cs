using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalariumSphere.Application.Features.Profiles.Commands;
using SalariumSphere.Application.Features.Profiles.Common;
using SalariumSphere.Application.Features.Profiles.Queries;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfilesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ProfileDto>>> GetProfiles([FromQuery] bool includeInactive = false)
    {
        var query = new GetProfilesQuery { IncludeInactive = includeInactive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProfileDto>> GetProfile(Guid id)
    {
        var query = new GetProfileByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProfileDto>> CreateProfile([FromBody] CreateProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProfile), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(Guid id, [FromBody] UpdateProfileCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProfile(Guid id)
    {
        var command = new DeleteProfileCommand { Id = id };
        await _mediator.Send(command);
        return NoContent();
    }
}