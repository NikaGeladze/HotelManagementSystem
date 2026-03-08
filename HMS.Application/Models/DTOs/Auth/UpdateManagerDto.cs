namespace HMS.Application.Models.DTOs.Auth;

public record UpdateManagerDto
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string PhoneNumber { get; init; }
    public string Email { get; init; }
}