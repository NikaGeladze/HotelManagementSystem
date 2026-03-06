namespace HMS.Application.Models.DTOs.Reservation;

public record CreateReservationDto(
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    List<Guid> RoomIds);