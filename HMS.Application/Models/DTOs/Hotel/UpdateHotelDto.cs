namespace HMS.Application.Models.DTOs.Hotel;

public record UpdateHotelDto(
    string Name,
    byte Rating,
    string Address);