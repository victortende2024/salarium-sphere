using MediatR;
using Microsoft.EntityFrameworkCore;
using SalariumSphere.Application.Features.Users.Common;
using SalariumSphere.Application.Features.Users.Queries;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;

namespace SalariumSphere.Application.Features.Users.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantResolver _tenantResolver;

    public CreateUserCommandHandler(IApplicationDbContext context, ITenantResolver tenantResolver)
    {
        _context = context;
        _tenantResolver = tenantResolver;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantResolver.GetCurrentTenantAsync();

        // Check if email already exists
        var existing = await _context.Set<User>()
            .AnyAsync(u => u.Email == request.Email && u.TenantId == tenant.Id, cancellationToken);
        if (existing)
            throw new Exception("A user with this email already exists.");

        // Verify employee exists if provided
        if (request.EmployeeId.HasValue)
        {
            var employee = await _context.Set<Employee>()
                .FirstOrDefaultAsync(e => e.Id == request.EmployeeId.Value && e.TenantId == tenant.Id, cancellationToken);
            if (employee == null)
                throw new Exception("Employee not found or does not belong to this tenant.");
        }

        // Generate password if not provided
        var password = request.Password ?? GenerateRandomPassword();
        var passwordHash = HashPassword(password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role,
            EmployeeId = request.EmployeeId,
            PasswordHash = passwordHash,
            TenantId = tenant.Id,
            IsActive = true
        };

        await _context.Set<User>().AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Re-fetch with Employee included
        var created = await _context.Set<User>()
            .Include(u => u.Employee)
            .FirstAsync(u => u.Id == user.Id, cancellationToken);

        return GetUsersQueryHandler.MapToDto(created);
    }

    private static string GenerateRandomPassword()
    {
        // Simple random password – 8 characters with uppercase, lowercase, digit
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string HashPassword(string password)
    {
        // For now, store plain text (we'll use a proper hash in production)
        // In production: BCrypt.Net.BCrypt.HashPassword(password)
        return password;
    }
}