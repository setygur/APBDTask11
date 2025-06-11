using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PositionsController> _logger;

    public PositionsController(AppDbContext context, ILogger<PositionsController> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PositionDto>>> GetAllPositions()
    {
        _logger.LogInformation($"GetAllPositions called at {DateTime.UtcNow}");
        var positions = await _context.Positions
            .Select(d => new PositionDto()
            {
                Id = d.Id,
                Name = d.Name
            }).ToListAsync();

        _logger.LogInformation($"GetAllPositions returned {positions.Count} positions at {DateTime.UtcNow}");
        return Ok(positions);
    }
}