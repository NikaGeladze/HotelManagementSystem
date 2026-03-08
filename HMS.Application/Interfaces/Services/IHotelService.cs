using HMS.Application.Models.DTOs.Hotel;

namespace HMS.Application.Interfaces.Services;

public interface IHotelService
{
    Task<Guid> CreateAsync(CreateHotelDto dto);
    Task UpdateAsync(Guid hotelId, UpdateHotelDto dto,string requesterId = null,bool isAdmin = false);
    Task DeleteAsync(Guid hotelId);
    Task<HotelDetailDto> GetByIdAsync(Guid hotelId);
    Task<List<HotelSummaryDto>> GetAllAsync(HotelFilterDto filter);
}