using HMS.Application.Models.DTOs.Auth;

namespace HMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<string> RegisterGuestAsync(RegisterGuestDto dto);
    Task<string> RegisterManagerAsync(Guid hotelId, RegisterManagerDto dto);
    Task<string> RegisterAdminAsync(RegisterAdminDto dto);
    Task<string> LoginAsync(LoginDto dto);
}