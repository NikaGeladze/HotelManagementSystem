using System.Net;
using System.Security.Claims;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models;
using HMS.Application.Models.DTOs.Reservation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAll([FromQuery] ReservationFilterDto filter)
    {
        if (User.IsInRole("Manager"))
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hotelId = await _reservationService.GetManagerHotelIdAsync(managerId);
            filter = filter with { HotelId = hotelId };
        }

        var reservations = await _reservationService.SearchAsync(filter);
        return Ok(new CommonResponse<List<ReservationResponseDto>>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Reservations retrieved successfully.",
            Result = reservations
        });
    }

    [HttpGet("my")]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> GetMyReservations([FromQuery] ReservationFilterDto filter)
    {
        var guestId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        filter = filter with { GuestId = guestId };
        var reservations = await _reservationService.SearchAsync(filter);
        return Ok(new CommonResponse<List<ReservationResponseDto>>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Reservations retrieved successfully.",
            Result = reservations
        });
    }

    [HttpGet("{reservationId:guid}")]
    public async Task<IActionResult> GetById(Guid reservationId)
    {
        var reservation = await _reservationService.GetByIdAsync(reservationId);
        return Ok(new CommonResponse<ReservationResponseDto>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Reservation retrieved successfully.",
            Result = reservation
        });
    }

    [HttpPut("{reservationId:guid}")]
    [Authorize(Roles = "Admin,Guest")]
    public async Task<IActionResult> Update(Guid reservationId, [FromBody] UpdateReservationDto dto)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        await _reservationService.UpdateAsync(requesterId, isAdmin, reservationId, dto);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Reservation updated successfully.",
            Result = null
        });
    }

    [HttpDelete("{reservationId:guid}")]
    [Authorize(Roles = "Admin,Guest")]
    public async Task<IActionResult> Delete(Guid reservationId)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        await _reservationService.DeleteAsync(requesterId, isAdmin, reservationId);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Reservation deleted successfully.",
            Result = null
        });
    }
}