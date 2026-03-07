namespace HMS.Application.Models.DTOs.Auth;

public record LoginDto
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string Password { get; init; }
}