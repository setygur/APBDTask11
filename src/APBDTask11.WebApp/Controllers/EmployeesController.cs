using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/employees")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(AppDbContext context, ILogger<EmployeesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetAllEmployees()
    {
        _logger.LogInformation($"GetAllEmployees called at {DateTime.UtcNow}");
        var employees = await _context.Employees
            .Include(e => e.Person)
            .Select(e => new EmployeeListDto
            {
                Id = e.Id,
                FullName = $"{e.Person.FirstName ?? ""} {e.Person.LastName ?? ""}".Trim()
            })
            .ToListAsync();

        _logger.LogInformation($"GetAllEmployees returned {employees.Count} employees");
        return Ok(employees);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<EmployeeDetailsDto>> GetEmployeeById(int id)
    {
        _logger.LogInformation($"GetEmployeeById{id} called at {DateTime.UtcNow}");
        var employee = await _context.Employees
            .Include(e => e.Person)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            _logger.LogWarning($"GetEmployeeById{id} employee was not found at {DateTime.UtcNow}");
            return NotFound();
        }

        _logger.LogInformation($"GetEmployeeById{id} returned {employee}");
        return Ok(new EmployeeDetailsDto
        {
            PassportNumber = employee.Person.PassportNumber,
            FirstName = employee.Person.FirstName,
            MiddleName = employee.Person.MiddleName,
            LastName = employee.Person.LastName,
            PhoneNumber = employee.Person.PhoneNumber,
            Email = employee.Person.Email,
            Salary = employee.Salary,
            HireDate = employee.HireDate,
            Position = new PositionDto
            {
                Id = employee.Position.Id,
                Name = employee.Position.Name
            }
        });
    }
}

