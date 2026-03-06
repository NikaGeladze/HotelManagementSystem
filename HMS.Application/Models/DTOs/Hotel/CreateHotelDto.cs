namespace HMS.Application.Models.DTOs.Hotel;

public record CreateHotelDto(
    string Name,
    byte Rating,
    string Country,
    string City,
    string Address);