using HMS.Application.Models.DTOs.Room;

namespace HMS.Application.Interfaces.Services;

public interface IRoomService
{
    Task<Guid> CreateAsync(Guid hotelId, CreateRoomDto dto);
    Task UpdateAsync(Guid hotelId, Guid roomId, UpdateRoomDto dto);
    Task DeleteAsync(Guid hotelId, Guid roomId);
    Task<RoomResponseDto> GetByIdAsync(Guid hotelId, Guid roomId);
    Task<List<RoomResponseDto>> SearchAsync(Guid hotelId, RoomSearchDto filter);
}