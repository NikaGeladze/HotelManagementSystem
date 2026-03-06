namespace HMS.Application.Models.DTOs.Hotel;

public record HotelSummaryDto(
    Guid Id,
    string Name,
    byte Rating,
    string Country,
    string City,
    string Address,
    int RoomCount);