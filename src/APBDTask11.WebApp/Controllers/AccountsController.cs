using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using APBDTask11.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<RegisterAccountDto> _passwordHasher = new();
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(AppDbContext context, ILogger<AccountsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RegisterAccount([FromBody] RegisterAccountDto dto)
    {
        _logger.LogInformation($"RegisterAccount called at {DateTime.UtcNow}");
        if (!ModelState.IsValid)
        {
            _logger.LogError($"RegisterAccount ModelState was not valid at {DateTime.UtcNow}");
            return BadRequest(ModelState);
        }

        var existing = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == dto.Username);
        if (existing != null)
        {
            _logger.LogWarning($"RegisterAccount already exists at {DateTime.UtcNow}");
            return Conflict("Username already exists.");
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.RoleName);
        if (role == null)
        {
            _logger.LogWarning($"RegisterAccount role was not found at {DateTime.UtcNow}");
            return BadRequest("Invalid role.");
        }

        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
        {
            _logger.LogWarning($"RegisterAccount employee was not found at {DateTime.UtcNow}");
            return NotFound("Employee not found.");
        }

        var account = new Account
        {
            Username = dto.Username,
            Password = _passwordHasher.HashPassword(dto, dto.Password),
            RoleId = role.Id,
            EmployeeId = dto.EmployeeId
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"RegisterAccount completed successfully for {account} at {DateTime.UtcNow}");
        return StatusCode(201);
    }
}