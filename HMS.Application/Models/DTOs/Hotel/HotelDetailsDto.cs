using HMS.Application.Models.DTOs.Room;

namespace HMS.Application.Models.DTOs.Hotel;

public record HotelDetailDto(
    Guid Id,
    string Name,
    byte Rating,
    string Country,
    string City,
    string Address,
    List<RoomResponseDto> Rooms);