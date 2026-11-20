using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Users.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public ResetPasswordCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.TenantId == tenant.Id, cancellationToken);
        if (user == null)
            throw new Exception($"User with ID {request.Id} not found.");

        user.PasswordHash = request.NewPassword; // In production: hash with BCrypt
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}