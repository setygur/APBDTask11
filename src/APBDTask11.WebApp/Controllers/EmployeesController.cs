using APBDTask11.Database;
using APBDTask11.Database.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APBDTask11.WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EmployeesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<EmployeeListDto>>> GetAllEmployees()
    {
        var employees = await _context.Employees
            .Include(e => e.Person)
            .Select(e => new EmployeeListDto
            {
                Id = e.Id,
                FullName = $"{e.Person.FirstName ?? ""} {e.Person.LastName ?? ""}".Trim()
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<EmployeeDetailsDto>> GetEmployeeById(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Person)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return NotFound();

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

