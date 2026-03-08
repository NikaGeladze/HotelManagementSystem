using HMS.Application.Exceptions;
using HMS.Application.Interfaces.Repositories;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models.DTOs.Reservation;
using HMS.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public ReservationService(
        IReservationRepository reservationRepository,
        IRoomRepository roomRepository,
        IHotelRepository hotelRepository,
        IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(string guestId, Guid hotelId, CreateReservationDto dto)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (dto.CheckInDate < today)
            throw new ValidationException(["Check-in date cannot be in the past."]);

        if (dto.CheckOutDate <= dto.CheckInDate)
            throw new ValidationException(["Check-out date must be after check-in date."]);
        
        var hotelExists = await _hotelRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExists)
            throw new NotFoundException($"Hotel with id {hotelId} not found.");
        
        var rooms = new List<Room>();
        foreach (var roomId in dto.RoomIds)
        {
            var room = await _roomRepository.GetAsync(
                r => r.Id == roomId && r.HotelId == hotelId,
                includes: q => q
                    .Include(r => r.ReservationRooms)
                        .ThenInclude(rr => rr.Reservation))
                ?? throw new NotFoundException($"Room with id {roomId} not found in hotel {hotelId}.");
            
            var isAvailable = !room.ReservationRooms.Any(rr =>
                rr.Reservation.CheckInDate < dto.CheckOutDate &&
                rr.Reservation.CheckOutDate > dto.CheckInDate);

            if (!isAvailable)
                throw new ConflictException($"Room with id {roomId} is not available for the selected dates.");

            rooms.Add(room);
        }
        
        var reservation = new Reservation
        {
            GuestId = guestId,
            CheckInDate = dto.CheckInDate,
            CheckOutDate = dto.CheckOutDate,
            ReservationRooms = dto.RoomIds.Select(roomId => new ReservationRoom
            {
                RoomId = roomId
            }).ToList()
        };

        await _reservationRepository.AddAsync(reservation);
        await _reservationRepository.SaveAsync();

        return reservation.Id;
    }

    public async Task UpdateAsync(string requesterId, bool isAdmin, Guid reservationId, UpdateReservationDto dto)
    {
        var reservation = await _reservationRepository.GetAsync(
                              r => r.Id == reservationId,
                              includes: q => q.Include(r => r.ReservationRooms))
                          ?? throw new NotFoundException($"Reservation with id {reservationId} not found.");

        // Admin can update any reservation, Guest only their own
        if (!isAdmin && reservation.GuestId != requesterId)
            throw new UnauthorizedException("You are not authorized to update this reservation.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (dto.CheckInDate < today)
            throw new ValidationException(["Check-in date cannot be in the past."]);

        if (dto.CheckOutDate <= dto.CheckInDate)
            throw new ValidationException(["Check-out date must be after check-in date."]);

        foreach (var reservationRoom in reservation.ReservationRooms)
        {
            var hasOverlap = await _reservationRepository.ExistsAsync(r =>
                r.Id != reservationId &&
                r.ReservationRooms.Any(rr => rr.RoomId == reservationRoom.RoomId) &&
                r.CheckInDate < dto.CheckOutDate &&
                r.CheckOutDate > dto.CheckInDate);

            if (hasOverlap)
                throw new ConflictException($"Room with id {reservationRoom.RoomId} is not available for the selected dates.");
        }

        reservation.CheckInDate = dto.CheckInDate;
        reservation.CheckOutDate = dto.CheckOutDate;

        _reservationRepository.Update(reservation);
        await _reservationRepository.SaveAsync();
    }

    public async Task DeleteAsync(string requesterId, bool isAdmin, Guid reservationId)
    {
        var reservation = await _reservationRepository.GetAsync(
                              r => r.Id == reservationId)
                          ?? throw new NotFoundException($"Reservation with id {reservationId} not found.");

        if (!isAdmin && reservation.GuestId != requesterId)
            throw new UnauthorizedException("You are not authorized to cancel this reservation.");

        _reservationRepository.Remove(reservation);
        await _reservationRepository.SaveAsync();
    }

    public async Task<ReservationResponseDto> GetByIdAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetAsync(
            r => r.Id == reservationId,
            includes: q => q
                .Include(r => r.Guest)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                        .ThenInclude(r => r.Hotel),
            tracking: false)
            ?? throw new NotFoundException($"Reservation with id {reservationId} not found.");

        return _mapper.Map<ReservationResponseDto>(reservation);
    }

    public async Task<List<ReservationResponseDto>> SearchAsync(ReservationFilterDto filter)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var (reservations, _) = await _reservationRepository.GetAllAsync(
            filter: r =>
                (filter.GuestId == null || r.GuestId == filter.GuestId) &&
                (filter.RoomId == null || r.ReservationRooms.Any(rr => rr.RoomId == filter.RoomId)) &&
                (filter.HotelId == null || r.ReservationRooms.Any(rr => rr.Room.HotelId == filter.HotelId)) &&
                (filter.From == null || r.CheckOutDate >= filter.From) &&
                (filter.To == null || r.CheckInDate <= filter.To) &&
                (filter.IsActive == null || filter.IsActive.Value
                    ? r.CheckOutDate >= today
                    : r.CheckOutDate < today),
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize,
            includes: q => q
                .Include(r => r.Guest)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Room)
                        .ThenInclude(r => r.Hotel),
            tracking: false);

        return _mapper.Map<List<ReservationResponseDto>>(reservations);
    }
    
    public async Task<Guid> GetManagerHotelIdAsync(string managerId)
    {
        var hotel = await _hotelRepository.GetAsync(
                        h => h.Managers.Any(m => m.Id == managerId),
                        tracking: false)
                    ?? throw new NotFoundException("No hotel found for this manager.");

        return hotel.Id;
    }
}