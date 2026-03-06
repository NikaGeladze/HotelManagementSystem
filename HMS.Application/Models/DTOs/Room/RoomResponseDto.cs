namespace HMS.Application.Models.DTOs.Room;

public record RoomResponseDto(
    Guid Id,
    string Name,
    decimal Price,
    Guid HotelId,
    string HotelName);