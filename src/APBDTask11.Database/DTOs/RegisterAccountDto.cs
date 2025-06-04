using System.ComponentModel.DataAnnotations;

namespace APBDTask11.Database.DTOs;

public class RegisterAccountDto
{
    [Required]
    [RegularExpression("^[^0-9][a-zA-Z0-9]*$", ErrorMessage = "Username must not start with a number.")]
    public string Username { get; set; }

    [Required]
    [MinLength(12)]
    [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password must have at least one lowercase, one uppercase, one digit and one special character.")]
    public string Password { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public string RoleName { get; set; }
}