namespace HMS.Application.Models.DTOs.Reservation;

public record UpdateReservationDto(
    DateOnly CheckInDate,
    DateOnly CheckOutDate);