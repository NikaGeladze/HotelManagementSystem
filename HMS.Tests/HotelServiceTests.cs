using System.Linq.Expressions;
using FluentAssertions;
using HMS.Application.Exceptions;
using HMS.Application.Interfaces.Repositories;
using HMS.Application.Models.DTOs.Hotel;
using HMS.Application.Services;
using HMS.Domain.Entities;
using MapsterMapper;
using Moq;

namespace HMS.Tests;

public class HotelServiceTests
{
    private readonly Mock<IHotelRepository> _hotelRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly HotelService _hotelService;

    public HotelServiceTests()
    {
        _hotelRepositoryMock = new Mock<IHotelRepository>();
        _mapperMock = new Mock<IMapper>();
        _hotelService = new HotelService(_hotelRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenHotelNotFound_ThrowsNotFoundException()
    {
        var hotelId = Guid.NewGuid();
        _hotelRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync((Hotel?)null);

        var act = async () => await _hotelService.GetByIdAsync(hotelId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Hotel with id {hotelId} not found.");
    }

    [Fact]
    public async Task DeleteAsync_WhenHotelHasRooms_ThrowsConflictException()
    {
        var hotel = new Hotel
        {
            Id = Guid.NewGuid(),
            Rooms = new List<Room> { new Room { Id = Guid.NewGuid() } }
        };

        _hotelRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Hotel, bool>>>(),
                It.IsAny<Func<IQueryable<Hotel>, IQueryable<Hotel>>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(hotel);

        var act = async () => await _hotelService.DeleteAsync(hotel.Id);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("Hotel cannot be deleted because it has rooms.");
    }

    [Fact]
    public async Task CreateAsync_WhenValidDto_ReturnsNewId()
    {
        var dto = new CreateHotelDto("Test Hotel", 4, "Georgia", "Tbilisi", "123 Street");
        var hotel = new Hotel { Id = Guid.NewGuid() };

        _mapperMock
            .Setup(m => m.Map<Hotel>(dto))
            .Returns(hotel);

        _hotelRepositoryMock
            .Setup(r => r.AddAsync(hotel))
            .Returns(Task.CompletedTask);

        _hotelRepositoryMock
            .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _hotelService.CreateAsync(dto);

        result.Should().Be(hotel.Id);
    }
}