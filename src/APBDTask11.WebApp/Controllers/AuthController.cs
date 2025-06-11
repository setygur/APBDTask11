using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using APBDTask11.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly PasswordHasher<Account> _passwordHasher = new();
    private readonly ILogger<AuthController> _logger;

    public AuthController(AppDbContext context, ITokenService tokenService, ILogger<AuthController> logger)
    {
        _context = context;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResponseDto>> Authenticate(AuthRequestDto dto, CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Authenticate called at {DateTime.UtcNow}");
        var foundUser = await _context.Accounts.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username.Equals(dto.Username), cancellationToken);

        if (foundUser == null)
        {
            _logger.LogWarning($"User {dto.Username} not found at {DateTime.UtcNow}");
            return Unauthorized();
        }
        
        var verificationResult = _passwordHasher.VerifyHashedPassword(foundUser, foundUser.Password, dto.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning($"User {dto.Username} provided incorrect password at {DateTime.UtcNow}");
            return Unauthorized();
        }
        var accessToken = _tokenService.GenerateToken(foundUser.Username, foundUser.Role.Name);

        _logger.LogInformation($"User {dto.Username} access token generated at {DateTime.UtcNow}");
        return new AuthResponseDto
        {
            Username = foundUser.Username,
            Role = foundUser.Role.Name,
            Token = accessToken
        };
    }
}