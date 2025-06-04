using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using APBDTask11.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<RegisterAccountDto> _passwordHasher = new();

    public AccountsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RegisterAccount([FromBody] RegisterAccountDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == dto.Username);
        if (existing != null)
            return Conflict("Username already exists.");

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == dto.RoleName);
        if (role == null)
            return BadRequest("Invalid role.");

        var employee = await _context.Employees.FindAsync(dto.EmployeeId);
        if (employee == null)
            return NotFound("Employee not found.");

        var account = new Account
        {
            Username = dto.Username,
            Password = _passwordHasher.HashPassword(dto, dto.Password),
            RoleId = role.Id,
            EmployeeId = dto.EmployeeId
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return StatusCode(201);
    }
}