namespace HMS.Application.Models.DTOs.Auth;

public record ManagerResponseDto(
    string Id,
    string FirstName,
    string LastName,
    string PersonalNumber,
    string Email,
    string PhoneNumber);