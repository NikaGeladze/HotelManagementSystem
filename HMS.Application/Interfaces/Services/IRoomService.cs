using HMS.Application.Models.DTOs.Room;

namespace HMS.Application.Interfaces.Services;

public interface IRoomService
{
    Task<Guid> CreateAsync(Guid hotelId, CreateRoomDto dto,string requesterId = null,bool isAdmin = false);
    Task UpdateAsync(Guid hotelId, Guid roomId, UpdateRoomDto dto,string requesterId = null,bool isAdmin = false);
    Task DeleteAsync(Guid hotelId, Guid roomId,string requesterId = null,bool isAdmin = false);
    Task<RoomResponseDto> GetByIdAsync(Guid hotelId, Guid roomId);
    Task<List<RoomResponseDto>> SearchAsync(Guid hotelId, RoomSearchDto filter);
}