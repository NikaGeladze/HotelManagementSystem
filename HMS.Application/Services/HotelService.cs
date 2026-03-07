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
        var hotel = _mapper.Map<Hotel>(dto);
        await _hotelRepository.AddAsync(hotel);
        await _hotelRepository.SaveAsync();
        return hotel.Id;
    }

    public async Task UpdateAsync(Guid hotelId, UpdateHotelDto dto)
    {
        var hotel = await _hotelRepository.GetAsync(
            h => h.Id == hotelId)
            ?? throw new NotFoundException($"Hotel with id {hotelId} not found.");

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
                .Include(h => h.Rooms),
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
            includes: q => q.Include(h => h.Rooms),
            tracking: false);

        return _mapper.Map<List<HotelSummaryDto>>(hotels);
    }
}