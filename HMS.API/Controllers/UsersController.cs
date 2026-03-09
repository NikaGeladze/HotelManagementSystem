using System.Net;
using System.Security.Claims;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models;
using HMS.Application.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IAuthService _authService;

    public UsersController(IAuthService authService)
    {
        _authService = authService;
    }

    // ── Manager ───────────────────────────────────────────────

    /// <summary>
    /// მენეჯერის განახლება
    /// </summary>]
    [HttpPut("managers/{managerId}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateManager(string managerId, [FromBody] UpdateManagerDto dto)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        await _authService.UpdateManagerAsync(managerId, requesterId, isAdmin, dto);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Manager updated successfully.",
            Result = null
        });
    }

    /// <summary>
    /// მენეჯერის წაშლა
    /// </summary>
    [HttpDelete("managers/{managerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteManager(string managerId)
    {
        await _authService.DeleteManagerAsync(managerId);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Manager deleted successfully.",
            Result = null
        });
    }
    

    //------getusers----
    /// <summary>
    /// ყველა მომხმარებლის ნახვა
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> GetAllUsers([FromQuery] string? role,[FromQuery] string? fullname)
    {
        string? userRole = User.FindFirstValue(ClaimTypes.Role);
        string? requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        List<UsersResponseDto> users = new List<UsersResponseDto>();
        if(userRole == "Admin")
            users = await _authService.GetAllUsers(role,fullname);
        if(userRole == "Manager")
            users = await _authService.GetAllUsers(role:"Guest",fullname:fullname,requesterId:requesterId);

        return Ok(new CommonResponse<List<UsersResponseDto>>
        {
            IsSuccess = true,
            Message = "Users retrieved Succesfully",
            Result = users,
            StatusCode = HttpStatusCode.OK
        });
    }

    /// <summary>
    /// მომხმარებლის განახლება
    /// </summary>
    [HttpPut("guests")]
    [HttpPut("guests/{guestId}")]
    [Authorize(Roles = "Admin,Guest")]
    public async Task<IActionResult> UpdateGuest([FromBody] UpdateGuestDto dto, string? guestId = null)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        var targetId = guestId ?? requesterId;

        if (!isAdmin && targetId != requesterId)
            throw new Application.Exceptions.UnauthorizedException("You can only update your own account.");
        if (isAdmin && string.IsNullOrEmpty(guestId)) throw new ArgumentException("Admin can not be updated");

        await _authService.UpdateGuestAsync(targetId, dto);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Guest updated successfully.",
            Result = null
        });
    }
    
    /// <summary>
    /// მომხმარებლის წაშლა
    /// </summary>
    [HttpDelete("guests/{guestId}")]
    [Authorize(Roles = "Admin,Guest")]
    public async Task<IActionResult> DeleteGuest(string guestId)
    {
        var requesterId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        await _authService.DeleteGuestAsync(guestId, requesterId, isAdmin);
        return Ok(new CommonResponse<object>
        {
            IsSuccess = true,
            StatusCode = HttpStatusCode.OK,
            Message = "Guest deleted successfully.",
            Result = null
        });
    }
}