using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using APBDTask11.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(AppDbContext context, ILogger<DevicesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<DeviceListDto>>> GetAllDevices()
    {
        _logger.LogInformation($"GetAllDevices called at {DateTime.UtcNow}");
        var devices = await _context.Devices
            .Select(d => new DeviceListDto
            {
                Id = d.Id,
                Name = d.Name
            }).ToListAsync();
        
        _logger.LogInformation($"GetAllDevices finished with result {devices} at {DateTime.UtcNow}");
        return Ok(devices);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<DeviceDetailsDto>> GetDeviceById(int id)
    {
        _logger.LogInformation($"GetDeviceById{id} called at {DateTime.UtcNow}");
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceEmployees.OrderByDescending(de => de.IssueDate))
                .ThenInclude(de => de.Employee)
                    .ThenInclude(e => e.Person)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
        {
            _logger.LogWarning($"GetDeviceById{id} was unable to find device at {DateTime.UtcNow}");
            return NotFound();
        }

        var latestAssignment = device.DeviceEmployees.FirstOrDefault(de => de.ReturnDate == null);

        _logger.LogInformation($"GetDeviceById{id} finished with result {latestAssignment} at {DateTime.UtcNow}");
        return Ok(new DeviceDetailsDto
        {
            Name = device.Name,
            DeviceTypeName = device.DeviceType?.Name,
            IsEnabled = device.IsEnabled,
            AdditionalProperties = string.IsNullOrEmpty(device.AdditionalProperties) 
                ? null 
                : System.Text.Json.JsonSerializer.Deserialize<object>(device.AdditionalProperties),
            CurrentEmployee = latestAssignment == null ? null : new DeviceEmployeeDto
            {
                Id = latestAssignment.Employee.Id,
                FullName = $"{latestAssignment.Employee.Person?.FirstName ?? ""} {latestAssignment.Employee.Person?.LastName ?? ""}".Trim()
            }
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> CreateDevice([FromBody] CreateOrUpdateDeviceDto dto)
    {
        _logger.LogInformation($"CreateDevice {dto.Name} called at {DateTime.UtcNow}");
        if (!ModelState.IsValid)
        {
            _logger.LogError($"CreateDevice {dto.Name} ModelState was not valid at {DateTime.UtcNow}");
            return BadRequest(ModelState);
        }

        var type = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Name == dto.DeviceTypeName);
        if (type == null)
        {
            _logger.LogWarning($"CreateDevice {dto.Name} Type was not found at {DateTime.UtcNow}");
            return BadRequest($"DeviceType '{dto.DeviceTypeName}' not found.");
        }

        var device = new Device
        {
            Name = dto.Name,
            IsEnabled = dto.IsEnabled,
            DeviceTypeId = type.Id,
            AdditionalProperties = System.Text.Json.JsonSerializer.Serialize(dto.AdditionalProperties)
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"CreateDevice {dto.Name} finished at {DateTime.UtcNow}");
        return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, null);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateDevice(int id, [FromBody] CreateOrUpdateDeviceDto dto)
    {
        _logger.LogInformation($"UpdateDevice {dto.Name} called at {DateTime.UtcNow}");
        if (!ModelState.IsValid)
        {
            _logger.LogWarning($"UpdateDevice {dto.Name} ModelState was not valid at {DateTime.UtcNow}");
            return BadRequest(ModelState);
        }
        
        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        if (device == null)
        {
            _logger.LogWarning($"UpdateDevice {dto.Name} device was not found at {DateTime.UtcNow}");
            return NotFound();
        }

        var type = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Name == dto.DeviceTypeName);
        if (type == null)
        {
            _logger.LogWarning($"UpdateDevice {dto.Name} Type was not found at {DateTime.UtcNow}");
            return BadRequest($"DeviceType '{dto.DeviceTypeName}' not found.");
        }

        device.Name = dto.Name;
        device.IsEnabled = dto.IsEnabled;
        device.DeviceTypeId = type.Id;
        device.AdditionalProperties = System.Text.Json.JsonSerializer.Serialize(dto.AdditionalProperties);

        await _context.SaveChangesAsync();

        _logger.LogInformation($"UpdateDevice {dto.Name} finished at {DateTime.UtcNow}");
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteDevice(int id)
    {
        _logger.LogInformation($"DeleteDevice {id} called at {DateTime.UtcNow}");
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            _logger.LogWarning($"DeleteDevice {id} device was not found at {DateTime.UtcNow}");
            return NotFound();
        }

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"DeleteDevice {id} finished at {DateTime.UtcNow}");
        return NoContent();
    }
}