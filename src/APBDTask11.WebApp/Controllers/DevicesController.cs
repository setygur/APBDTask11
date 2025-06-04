using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using APBDTask11.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public DevicesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<DeviceListDto>>> GetAllDevices()
    {
        var devices = await _context.Devices
            .Select(d => new DeviceListDto
            {
                Id = d.Id,
                Name = d.Name
            }).ToListAsync();

        return Ok(devices);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<DeviceDetailsDto>> GetDeviceById(int id)
    {
        var device = await _context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceEmployees.OrderByDescending(de => de.IssueDate))
                .ThenInclude(de => de.Employee)
                    .ThenInclude(e => e.Person)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (device == null)
            return NotFound();

        var latestAssignment = device.DeviceEmployees.FirstOrDefault(de => de.ReturnDate == null);

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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var type = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Name == dto.DeviceTypeName);
        if (type == null)
            return BadRequest($"DeviceType '{dto.DeviceTypeName}' not found.");

        var device = new Device
        {
            Name = dto.Name,
            IsEnabled = dto.IsEnabled,
            DeviceTypeId = type.Id,
            AdditionalProperties = System.Text.Json.JsonSerializer.Serialize(dto.AdditionalProperties)
        };

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDeviceById), new { id = device.Id }, null);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> UpdateDevice(int id, [FromBody] CreateOrUpdateDeviceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == id);
        if (device == null)
            return NotFound();

        var type = await _context.DeviceTypes.FirstOrDefaultAsync(t => t.Name == dto.DeviceTypeName);
        if (type == null)
            return BadRequest($"DeviceType '{dto.DeviceTypeName}' not found.");

        device.Name = dto.Name;
        device.IsEnabled = dto.IsEnabled;
        device.DeviceTypeId = type.Id;
        device.AdditionalProperties = System.Text.Json.JsonSerializer.Serialize(dto.AdditionalProperties);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
            return NotFound();

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}