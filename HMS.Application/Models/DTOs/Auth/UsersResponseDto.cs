namespace HMS.Application.Models.DTOs.Auth;

public record UsersResponseDto
{
    public string Id { get; init; }
    public string FullName { get; init; }
    public string PhoneNumber { get; init; }
    
    public ICollection<string> Roles { get; init; }
}