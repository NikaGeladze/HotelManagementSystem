namespace HMS.Application.Models.DTOs.Auth;

public record UpdateGuestDto
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string PhoneNumber { get; init; }
}