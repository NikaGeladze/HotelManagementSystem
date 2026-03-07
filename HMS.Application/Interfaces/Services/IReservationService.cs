using HMS.Application.Models.DTOs.Reservation;

namespace HMS.Application.Interfaces.Services;

public interface IReservationService
{
    Task<Guid> CreateAsync(string guestId, Guid hotelId, CreateReservationDto dto);
    Task UpdateAsync(string guestId, Guid reservationId, UpdateReservationDto dto);
    Task DeleteAsync(string guestId, Guid reservationId);
    Task<ReservationResponseDto> GetByIdAsync(Guid reservationId);
    Task<List<ReservationResponseDto>> SearchAsync(ReservationFilterDto filter);
}