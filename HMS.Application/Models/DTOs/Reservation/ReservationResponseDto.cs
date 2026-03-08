using HMS.Application.Models.DTOs.Room;

namespace HMS.Application.Models.DTOs.Reservation;

public record ReservationResponseDto(
    Guid Id,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string GuestFullName,
    string GuestId,
    List<RoomResponseDto> Rooms);