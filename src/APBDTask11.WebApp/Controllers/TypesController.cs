using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/devices/types")]
public class TypesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<TypesController> _logger;

    public TypesController(AppDbContext context, ILogger<TypesController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TypeDto>>> GetAllTypes()
    {
        _logger.LogDebug($"GetAllTypes called at {DateTime.UtcNow}");
        var types = await _context.DeviceTypes
            .Select(d => new TypeDto()
            {
                Id = d.Id,
                Name = d.Name
            }).ToListAsync();

        _logger.LogInformation($"GetAllTypes returned {types.Count} types at {DateTime.UtcNow}");
        return Ok(types);
    }
}