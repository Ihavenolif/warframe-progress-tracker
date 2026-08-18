using System.ComponentModel.DataAnnotations;

namespace rest_api.DTO;

public class LoginRequestDto
{
    [Required]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
