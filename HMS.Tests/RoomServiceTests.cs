using System.Linq.Expressions;
using FluentAssertions;
using HMS.Application.Exceptions;
using HMS.Application.Interfaces.Repositories;
using HMS.Application.Services;
using HMS.Domain.Entities;
using MapsterMapper;
using Moq;

namespace HMS.Tests;

public class RoomServiceTests
{
    private readonly Mock<IRoomRepository> _roomRepositoryMock;
    private readonly Mock<IHotelRepository> _hotelRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly RoomService _roomService;

    public RoomServiceTests()
    {
        _roomRepositoryMock = new Mock<IRoomRepository>();
        _hotelRepositoryMock = new Mock<IHotelRepository>();
        _mapperMock = new Mock<IMapper>();
        _roomService = new RoomService(
            _roomRepositoryMock.Object,
            _hotelRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Fact]
    public async Task DeleteAsync_WhenRoomHasActiveReservations_ThrowsConflictException()
    {
        var hotelId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var requesterId = "requesterId";
        
        _hotelRepositoryMock
            .Setup(r => r.ExistsAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(true);

        var room = new Room
        {
            Id = roomId,
            HotelId = hotelId,
            ReservationRooms = new List<ReservationRoom>
            {
                new ReservationRoom
                {
                    Reservation = new Reservation
                    {
                        CheckOutDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2))
                    }
                }
            }
        };

        _roomRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Room, bool>>>(),
                It.IsAny<Func<IQueryable<Room>, IQueryable<Room>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(room);
        
        var act = async () => await _roomService.DeleteAsync(hotelId, roomId, requesterId, false);
        
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Room cannot be deleted because it has active or future reservations.");
    }
}