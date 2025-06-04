using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using APBDTask11.Database.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher<Account> _passwordHasher = new();

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost]
    public async Task<ActionResult<AuthResponseDto>> Authenticate(AuthRequestDto dto, CancellationToken cancellationToken)
    {
        var foundUser = await _context.Accounts.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username.Equals(dto.Username), cancellationToken);

        if (foundUser == null)
        {
            return Unauthorized();
        }
        
        var verificationResult = _passwordHasher.VerifyHashedPassword(foundUser, foundUser.Password, dto.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }
        var accessToken = _tokenService.GenerateToken(foundUser.Username, foundUser.Role.Name);

        return new AuthResponseDto
        {
            Username = foundUser.Username,
            Role = foundUser.Role.Name,
            Token = accessToken
        };
    }
}