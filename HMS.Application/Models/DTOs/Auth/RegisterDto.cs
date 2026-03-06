namespace HMS.Application.Models.DTOs.Auth;

public record RegisterDto
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string PersonalNumber { get; init; }
    public string PhoneNumber { get; init; }
    public string Password { get; init; }
}