using HMS.Application.Models.DTOs.Reservation;

namespace HMS.Application.Interfaces.Services;

public interface IReservationService
{
    Task<Guid> CreateAsync(string guestId, Guid hotelId, CreateReservationDto dto);
    Task UpdateAsync(string requesterId, bool isAdmin, Guid reservationId, UpdateReservationDto dto);
    Task DeleteAsync(string requesterId, bool isAdmin, Guid reservationId);
    Task<ReservationResponseDto> GetByIdAsync(Guid reservationId);
    Task<List<ReservationResponseDto>> SearchAsync(ReservationFilterDto filter);
    Task<Guid> GetManagerHotelIdAsync(string managerId);
}