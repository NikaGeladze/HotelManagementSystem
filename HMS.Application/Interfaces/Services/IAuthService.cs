using HMS.Application.Models.DTOs.Auth;

namespace HMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<string> RegisterGuestAsync(RegisterGuestDto dto);
    Task<string> RegisterManagerAsync(Guid hotelId, RegisterManagerDto dto);
    Task<string> RegisterAdminAsync(RegisterAdminDto dto);
    Task<string> LoginAsync(LoginDto dto);
    Task<List<UsersResponseDto>> GetAllUsers(string? role,string? fullname,string? requesterId = null);
    Task UpdateManagerAsync(string managerId, string requesterId, bool isAdmin, UpdateManagerDto dto);
    Task DeleteManagerAsync(string managerId);
    Task UpdateGuestAsync(string requesterId, UpdateGuestDto dto);      
    Task DeleteGuestAsync(string guestId, string requesterId, bool isAdmin);
    Task AssignManagerToHotelAsync(string managerId, Guid hotelId);
}