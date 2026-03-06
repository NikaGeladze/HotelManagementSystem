namespace HMS.Application.Models.DTOs.Auth;

public record RegisterManagerDto : RegisterDto
{
    public string Email { get; init; }
}