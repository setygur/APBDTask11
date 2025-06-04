using System.ComponentModel.DataAnnotations;

namespace APBDTask11.Database.DTOs;

public class AuthRequestDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Password { get; set; }
}