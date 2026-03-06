namespace HMS.Application.Models.DTOs.Room;

public record RoomSearchDto
{
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public DateOnly? AvailableFrom { get; init; }
    public DateOnly? AvailableTo { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}