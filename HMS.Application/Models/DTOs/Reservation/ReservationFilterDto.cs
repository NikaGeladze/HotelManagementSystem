namespace HMS.Application.Models.DTOs.Reservation;

public record ReservationFilterDto
{
    public Guid? HotelId { get; init; }
    public string? GuestId { get; init; }
    public Guid? RoomId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public bool? IsActive { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}