using HMS.Application.Exceptions;
using HMS.Application.Interfaces.Repositories;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models.DTOs.Hotel;
using HMS.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Services;

public class HotelService : IHotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public HotelService(
        IHotelRepository hotelRepository,
        IMapper mapper)
    {
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(CreateHotelDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5) throw new ValidationException(["Rating must be in range 1-5"]);
        var hotel = _mapper.Map<Hotel>(dto);
        await _hotelRepository.AddAsync(hotel);
        await _hotelRepository.SaveAsync();
        return hotel.Id;
    }

    public async Task UpdateAsync(Guid hotelId, UpdateHotelDto dto, string? requesterId = null, bool isAdmin = false)
    {
        if (dto.Rating < 1 || dto.Rating > 5) throw new ValidationException(["Rating must be in range 1-5"]);
        var hotel = await _hotelRepository.GetAsync(
                        h => h.Id == hotelId,
                        includes: q => q.Include(h => h.Managers)
                        )
                    ?? throw new NotFoundException($"Hotel with id {hotelId} not found.");

        if (!isAdmin)
        {
            var isOwnHotel = hotel.Managers.Any(m => m.Id == requesterId);
            if (!isOwnHotel)
                throw new UnauthorizedException("You can only update your own hotel.");
        }

        _mapper.Map(dto, hotel);
        _hotelRepository.Update(hotel);
        await _hotelRepository.SaveAsync();
    }

    public async Task DeleteAsync(Guid hotelId)
    {
        var hotel = await _hotelRepository.GetAsync(
            h => h.Id == hotelId,
            includes: q => q
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.ReservationRooms)
                        .ThenInclude(rr => rr.Reservation))
            ?? throw new NotFoundException($"Hotel with id {hotelId} not found.");

        if (hotel.Rooms.Any())
            throw new ConflictException("Hotel cannot be deleted because it has rooms.");

        var hasActiveReservations = hotel.Rooms
            .SelectMany(r => r.ReservationRooms)
            .Any(rr => rr.Reservation.CheckOutDate >= DateOnly.FromDateTime(DateTime.UtcNow));

        if (hasActiveReservations)
            throw new ConflictException("Hotel cannot be deleted because it has active reservations.");

        _hotelRepository.Remove(hotel);
        await _hotelRepository.SaveAsync();
    }

    public async Task<HotelDetailDto> GetByIdAsync(Guid hotelId)
    {
        var hotel = await _hotelRepository.GetAsync(
            h => h.Id == hotelId,
            includes: q => q
                .Include(h => h.Rooms)
                .Include(h => h.Managers),
            tracking: false)
            ?? throw new NotFoundException($"Hotel with id {hotelId} not found.");

        return _mapper.Map<HotelDetailDto>(hotel);
    }

    public async Task<List<HotelSummaryDto>> GetAllAsync(HotelFilterDto filter)
    {
        var (hotels, _) = await _hotelRepository.GetAllAsync(
            filter: h =>
                (filter.Country == null || h.Country == filter.Country) &&
                (filter.City == null || h.City == filter.City) &&
                (filter.Rating == null || h.Rating == filter.Rating),
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize,
            includes: q => q.Include(h => h.Rooms).Include(h=> h.Managers),
            tracking: false);
        if (hotels.Count == 0) throw new NotFoundException("Hotels not found");
        return _mapper.Map<List<HotelSummaryDto>>(hotels);
    }
}