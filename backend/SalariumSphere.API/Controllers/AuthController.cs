using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SalariumSphere.Application.Common;
using SalariumSphere.Application.Interfaces;
using SalariumSphere.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using BC = BCrypt.Net.BCrypt;

namespace SalariumSphere.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, IApplicationDbContext context, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _context = context;
        _logger = logger;
        _context = context;
    }

    [HttpGet("test-token")]
    public IActionResult GetTestToken()
    {
        // FOR TESTING ONLY – REPLACE WITH REAL AUTH IN PRODUCTION
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            new Claim("tenant_id", "11111111-1111-1111-1111-111111111111"), // Matches the seeded tenant
            new Claim(ClaimTypes.Role, "PayrollManager")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt: {Email}", request.Email);
            // 1. Find the user by email
            var user = await _context.Set<User>()
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("❌ User NOT FOUND in DB for {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // 2. Verify the password (in production, use BCrypt or similar)
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("❌ Invalid password for user {Email}", request.Email);
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // 3. Update last login time
            _logger.LogInformation("Login succeeded for user {UserId}", user.Id);
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

        
            // 4. Generate JWT token
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("employee_id", user.EmployeeId.ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new LoginResponse
            {
                Token = tokenString,
                Email = user.Email,
                Role = user.Role,
                UserId = user.Id,
                EmployeeId = user.EmployeeId,
                TenantId = user.TenantId,
                
            });
        }
        catch (Exception ex)
        {
            // Log the full exception
            Console.WriteLine($"Login error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
        }
    }

    // Temporary password verification – replace with BCrypt in production
    private bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash)) return false;
        try
        {
            return BC.Verify(password, hash);
        }
        catch
        {
            // Malformed hash in DB (e.g. leftover plaintext from an old seed)
            return false;
        }
    }
}