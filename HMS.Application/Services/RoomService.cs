using HMS.Application.Exceptions;
using HMS.Application.Interfaces.Repositories;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models.DTOs.Room;
using HMS.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly IHotelRepository _hotelRepository;
    private readonly IMapper _mapper;

    public RoomService(
        IRoomRepository roomRepository,
        IHotelRepository hotelRepository,
        IMapper mapper)
    {
        _roomRepository = roomRepository;
        _hotelRepository = hotelRepository;
        _mapper = mapper;
    }

    public async Task<Guid> CreateAsync(Guid hotelId, CreateRoomDto dto,string requesterId = null,bool isAdmin = false)
    {
        if (!isAdmin)
            await EnsureManagerOwnsHotelAsync(hotelId, requesterId);
        
        var hotelExists = await _hotelRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExists)
            throw new NotFoundException($"Hotel with id {hotelId} not found.");
        if (dto.Price <= 0)
            throw new ValidationException(["Room price is not positive!"]);

        var room = _mapper.Map<Room>(dto);
        room.HotelId = hotelId;

        await _roomRepository.AddAsync(room);
        await _roomRepository.SaveAsync();

        return room.Id;
    }

    public async Task UpdateAsync(Guid hotelId, Guid roomId, UpdateRoomDto dto,string requesterId = null,bool isAdmin = false)
    {
        if (!isAdmin)
            await EnsureManagerOwnsHotelAsync(hotelId, requesterId);
        
        if (dto.Price <= 0) throw new ValidationException(["Room price is not positive!"]);
        var room = await _roomRepository.GetAsync(
            r => r.Id == roomId && r.HotelId == hotelId)
            ?? throw new NotFoundException($"Room with id {roomId} not found in hotel {hotelId}.");

        _mapper.Map(dto, room);
        _roomRepository.Update(room);
        await _roomRepository.SaveAsync();
    }

    public async Task DeleteAsync(Guid hotelId, Guid roomId,string requesterId = null,bool isAdmin = false)
    {
        if (!isAdmin)
            await EnsureManagerOwnsHotelAsync(hotelId, requesterId);
        
        var room = await _roomRepository.GetAsync(
            r => r.Id == roomId && r.HotelId == hotelId,
            includes: q => q
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Reservation))
            ?? throw new NotFoundException($"Room with id {roomId} not found in hotel {hotelId}.");

        var hasActiveOrFutureReservations = room.ReservationRooms
            .Any(rr => rr.Reservation.CheckOutDate >= DateOnly.FromDateTime(DateTime.UtcNow));

        if (hasActiveOrFutureReservations)
            throw new ConflictException("Room cannot be deleted because it has active or future reservations.");

        _roomRepository.Remove(room);
        await _roomRepository.SaveAsync();
    }

    public async Task<RoomResponseDto> GetByIdAsync(Guid hotelId, Guid roomId)
    {
        var room = await _roomRepository.GetAsync(
            r => r.Id == roomId && r.HotelId == hotelId,
            includes: q => q.Include(r => r.Hotel),
            tracking: false)
            ?? throw new NotFoundException($"Room with id {roomId} not found in hotel {hotelId}.");

        return _mapper.Map<RoomResponseDto>(room);
    }

    public async Task<List<RoomResponseDto>> SearchAsync(Guid hotelId, RoomSearchDto filter)
    {
        var hotelExists = await _hotelRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExists)
            throw new NotFoundException($"Hotel with id {hotelId} not found.");

        var (rooms, _) = await _roomRepository.GetAllAsync(
            filter: r =>
                r.HotelId == hotelId &&
                (filter.MinPrice == null || r.Price >= filter.MinPrice) &&
                (filter.MaxPrice == null || r.Price <= filter.MaxPrice) &&
                (filter.AvailableFrom == null || filter.AvailableTo == null ||
                    !r.ReservationRooms.Any(rr =>
                        rr.Reservation.CheckInDate < filter.AvailableTo &&
                        rr.Reservation.CheckOutDate > filter.AvailableFrom)),
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize,
            includes: q => q
                .Include(r => r.Hotel)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Reservation),
            tracking: false);
        if (rooms.Count == 0) throw new NotFoundException("Rooms not found");

        return _mapper.Map<List<RoomResponseDto>>(rooms);
    }
    
    private async Task EnsureManagerOwnsHotelAsync(Guid hotelId, string requesterId)
    {
        var isOwnHotel = await _hotelRepository.ExistsAsync(
            h => h.Id == hotelId && h.Managers.Any(m => m.Id == requesterId));

        if (!isOwnHotel)
            throw new UnauthorizedException("You can only manage rooms of your own hotel.");
    }
}