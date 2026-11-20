using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Users.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public DeleteUserCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.TenantId == tenant.Id, cancellationToken);
        if (user == null)
            throw new Exception($"User with ID {request.Id} not found.");

        user.IsActive = false;
        user.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}