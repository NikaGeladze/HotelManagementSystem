namespace HMS.Application.Models.DTOs.Auth;

public record RegisterAdminDto : RegisterDto
{
    public string Email { get; init; }
}