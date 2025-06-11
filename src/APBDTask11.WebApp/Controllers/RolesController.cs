using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<RolesController> _logger;

    public RolesController(AppDbContext context, ILogger<RolesController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles()
    {
        _logger.LogDebug($"GetAllRoles called at {DateTime.UtcNow}");
        var roles = await _context.Roles
            .Select(d => new RoleDto()
            {
                Id = d.Id,
                Name = d.Name
            }).ToListAsync();

        _logger.LogDebug($"GetAllRoles returned {roles.Count} roles at {DateTime.UtcNow}");
        return Ok(roles);
    }
}