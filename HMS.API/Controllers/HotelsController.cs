using System.Net;
using System.Security.Claims;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models;
using HMS.Application.Models.DTOs.Auth;
using HMS.Application.Models.DTOs.Hotel;
using HMS.Application.Models.DTOs.Reservation;
using HMS.Application.Models.DTOs.Room;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers;

[ApiController]
[Route("api/hotels")]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;
    private readonly IRoomService _roomService;
    private readonly IAuthService _authService;
    private readonly IReservationService _reservationService;

    public HotelsController(
        IHotelService hotelService,
        IRoomService roomService,
        IAuthService authService,
        IReservationService reservationService)
    {
        _hotelService = hotelService;
        _roomService = roomService;
        _authService = authService;
        _reservationService = reservationService;
    }

    // ── Hotels ────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] HotelFilterDto filter)
    {
        var hotels = await _hotelService.GetAllAsync(filter);
        return Ok(new CommonResponse<List<HotelSummaryDto>>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Hotels retrieved successfully.",
            Result = hotels
        });
    }

    [HttpGet("{hotelId:guid}")]
    public async Task<IActionResult> GetById(Guid hotelId)
    {
        var hotel = await _hotelService.GetByIdAsync(hotelId);
        return Ok(new CommonResponse<HotelDetailDto>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Hotel retrieved successfully.",
            Result = hotel
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateHotelDto dto)
    {
        var id = await _hotelService.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, new CommonResponse<Guid>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Hotel created successfully.",
            Result = id
        });
    }

    [HttpPut("{hotelId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid hotelId, [FromBody] UpdateHotelDto dto)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        await _hotelService.UpdateAsync(hotelId, dto, requesterId, isAdmin);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Hotel updated successfully.",
            Result = null
        });
    }

    [HttpDelete("{hotelId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid hotelId)
    {
        await _hotelService.DeleteAsync(hotelId);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Hotel deleted successfully.",
            Result = null
        });
    }
    
    [HttpPut("{hotelId:guid}/managers/{managerId}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignManager(Guid hotelId, string managerId)
    {
        await _authService.AssignManagerToHotelAsync(managerId, hotelId);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Manager assigned to hotel successfully.",
            Result = null
        });
    }

    // ── Rooms ─────────────────────────────────────────────────

    [HttpGet("{hotelId:guid}/rooms")]
    public async Task<IActionResult> GetRooms(Guid hotelId, [FromQuery] RoomSearchDto filter)
    {
        var rooms = await _roomService.SearchAsync(hotelId, filter);
        return Ok(new CommonResponse<List<RoomResponseDto>>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Rooms retrieved successfully.",
            Result = rooms
        });
    }

    [HttpGet("{hotelId:guid}/rooms/{roomId:guid}")]
    public async Task<IActionResult> GetRoom(Guid hotelId, Guid roomId)
    {
        var room = await _roomService.GetByIdAsync(hotelId, roomId);
        return Ok(new CommonResponse<RoomResponseDto>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Room retrieved successfully.",
            Result = room
        });
    }

    [HttpPost("{hotelId:guid}/rooms")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateRoom(Guid hotelId, [FromBody] CreateRoomDto dto)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        var id = await _roomService.CreateAsync(hotelId, dto,requesterId, isAdmin);
        return StatusCode(201, new CommonResponse<Guid>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Room created successfully.",
            Result = id
        });
    }

    [HttpPut("{hotelId:guid}/rooms/{roomId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateRoom(Guid hotelId, Guid roomId, [FromBody] UpdateRoomDto dto)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        await _roomService.UpdateAsync(hotelId, roomId, dto,requesterId, isAdmin);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Room updated successfully.",
            Result = null
        });
    }

    [HttpDelete("{hotelId:guid}/rooms/{roomId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteRoom(Guid hotelId, Guid roomId)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");
        
        await _roomService.DeleteAsync(hotelId, roomId,requesterId, isAdmin);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Room deleted successfully.",
            Result = null
        });
    }

    // ── Reservations ──────────────────────────────────────────

    [HttpPost("{hotelId:guid}/reservations")]
    [Authorize(Roles = "Guest,Manager,Admin")]
    public async Task<IActionResult> CreateReservation(Guid hotelId, [FromBody] CreateReservationDto dto)
    {
        string guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var id = await _reservationService.CreateAsync(guestId, hotelId, dto);
        return StatusCode(201, new CommonResponse<Guid>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Reservation created successfully.",
            Result = id
        });
    }
}