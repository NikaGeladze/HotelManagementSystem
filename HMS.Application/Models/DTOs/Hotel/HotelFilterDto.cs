namespace HMS.Application.Models.DTOs.Hotel;

public record HotelFilterDto
{
    public string? Country { get; init; }
    public string? City { get; init; }
    public byte? Rating { get; init; }
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
}
