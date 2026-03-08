using System.Net;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models;
using HMS.Application.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HMS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/guest")]
    public async Task<IActionResult> RegisterGuest([FromBody] RegisterGuestDto dto)
    {
        var id = await _authService.RegisterGuestAsync(dto);
        return StatusCode(201, new CommonResponse<string>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Guest registered successfully.",
            Result = id
        });
    }

    [HttpPost("register/admin")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDto dto)
    {
        var id = await _authService.RegisterAdminAsync(dto);
        return StatusCode(201, new CommonResponse<string>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Admin registered successfully.",
            Result = id
        });
    }
    
    [HttpPost("register/manager/{hotelId:guid}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterManager(Guid hotelId, [FromBody] RegisterManagerDto dto)
    {
        var id = await _authService.RegisterManagerAsync(hotelId, dto);
        return StatusCode(201, new CommonResponse<string>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.Created,
            Message = "Manager registered successfully.",
            Result = id
        });
    }

    [HttpPost("login")]
    [EnableRateLimiting("ApiPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        return Ok(new CommonResponse<string>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Login successful.",
            Result = token
        });
    }
}