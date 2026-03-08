using HMS.Application.Exceptions;
using HMS.Application.Interfaces.Repositories;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models.DTOs.Auth;
using HMS.Application.Models.DTOs.Reservation;
using HMS.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace HMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHotelRepository _hotelRepository;
    private readonly IReservationService _reservationService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IHotelRepository hotelRepository,
        IReservationService reservationService
        )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _hotelRepository = hotelRepository;
        _reservationService = reservationService;
    }
    
    public async Task<string> RegisterGuestAsync(RegisterGuestDto dto)
    {
        await EnsureRolesExistAsync();
        await EnsurePersonalNumberUniqueAsync(dto.PersonalNumber);
        await EnsurePhoneNumberUniqueAsync(dto.PhoneNumber);

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PersonalNumber = dto.PersonalNumber,
            PhoneNumber = dto.PhoneNumber,
            UserName = dto.PhoneNumber
        };

        await CreateUserAsync(user, dto.Password);
        await _userManager.AddToRoleAsync(user, "Guest");

        return user.Id;
    }

    public async Task<string> RegisterManagerAsync(Guid hotelId, RegisterManagerDto dto)
    {
        await EnsureRolesExistAsync();
        await EnsurePersonalNumberUniqueAsync(dto.PersonalNumber);
        await EnsurePhoneNumberUniqueAsync(dto.PhoneNumber);
        await EnsureEmailUniqueAsync(dto.Email);

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PersonalNumber = dto.PersonalNumber,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            UserName = dto.Email,
            HotelId = hotelId
        };

        await CreateUserAsync(user, dto.Password);
        await _userManager.AddToRoleAsync(user, "Manager");

        return user.Id;
    }

    public async Task<string> RegisterAdminAsync(RegisterAdminDto dto)
    {
        await EnsureRolesExistAsync();
        await EnsureEmailUniqueAsync(dto.Email);

        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PersonalNumber = dto.PersonalNumber
        };

        await CreateUserAsync(user, dto.Password);
        await _userManager.AddToRoleAsync(user, "Admin");

        return user.Id;
    }

    public async Task<string> LoginAsync(LoginDto dto)
    {
        ApplicationUser? user = null;

        if (!string.IsNullOrEmpty(dto.Email))
            user = await _userManager.FindByEmailAsync(dto.Email);
        else if (!string.IsNullOrEmpty(dto.PhoneNumber))
            user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (user == null)
            throw new ArgumentException("User not found.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            throw new ArgumentException("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        return _jwtTokenGenerator.GenerateToken(user, roles);
    }

    public async Task<List<UsersResponseDto>> GetAllUsers(string? role,string? fullName,string? requesterId = null)
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<UsersResponseDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrEmpty(role))
            {
                roles = roles.Where(r => r.Equals(role)).ToList();
                if(roles.Count == 0) continue;
            }

            if (!string.IsNullOrEmpty(fullName))
            {
                if(fullName != user.FirstName + " " + user.LastName) continue;
            }

            if (!string.IsNullOrEmpty(requesterId))
            {
                var reservations = await _reservationService.SearchAsync(new ReservationFilterDto()
                {
                    HotelId = _userManager.Users.Where(u => u.Id == requesterId).ToList()[0].HotelId,
                    GuestId = user.Id
                });
                if(reservations.Count == 0) continue;
            }

            result.Add(new UsersResponseDto()
            {
                Id = user.Id,
                FullName = user.FirstName + " " + user.LastName,
                Roles = roles,
                PhoneNumber = user.PhoneNumber
            });
        }

        if (result.Count == 0) throw new NotFoundException("Users not found");

        return result;
    }


    public async Task AssignManagerToHotelAsync(string managerId, Guid hotelId)
    {
        var manager = await _userManager.FindByIdAsync(managerId)
                      ?? throw new NotFoundException($"Manager with id {managerId} not found.");

        var isManager = await _userManager.IsInRoleAsync(manager, "Manager");
        if (!isManager)
            throw new Exceptions.ValidationException(["User is not a manager."]);

        var hotelExists = await _hotelRepository.ExistsAsync(h => h.Id == hotelId);
        if (!hotelExists)
            throw new NotFoundException($"Hotel with id {hotelId} not found.");

        manager.HotelId = hotelId;

        var result = await _userManager.UpdateAsync(manager);
        if (!result.Succeeded)
            throw new Exceptions.ValidationException(result.Errors.Select(e => e.Description));
    }
    
    public async Task UpdateManagerAsync(string managerId, string requesterId, bool isAdmin, UpdateManagerDto dto)
    {
        var manager = await _userManager.FindByIdAsync(managerId)
                      ?? throw new NotFoundException($"Manager with id {managerId} not found.");
        
        if (!isAdmin && managerId != requesterId)
            throw new UnauthorizedException("You can only update your own account.");

        manager.FirstName = dto.FirstName ?? manager.FirstName;
        manager.LastName = dto.LastName ?? manager.LastName;
        manager.PhoneNumber = dto.PhoneNumber ?? manager.PhoneNumber;
        manager.Email = dto.Email ?? manager.Email;
        manager.UserName = dto.Email ?? manager.UserName;

        var result = await _userManager.UpdateAsync(manager);
        if (!result.Succeeded)
            throw new Exceptions.ValidationException(result.Errors.Select(e => e.Description));
    }

    public async Task DeleteManagerAsync(string managerId)
    {
        var manager = await _userManager.FindByIdAsync(managerId)
                      ?? throw new NotFoundException($"Manager with id {managerId} not found.");

        var hotelManagerCount = await _userManager.Users
            .CountAsync(u => u.HotelId == manager.HotelId);

        if (hotelManagerCount <= 1)
            throw new ConflictException("Cannot delete the only manager of a hotel.");

        var result = await _userManager.DeleteAsync(manager);
        if (!result.Succeeded)
            throw new Exceptions.ValidationException(result.Errors.Select(e => e.Description));
    }
    
    public async Task UpdateGuestAsync(string requesterId, UpdateGuestDto dto)
    {
        var guest = await _userManager.FindByIdAsync(requesterId)
                    ?? throw new NotFoundException("Guest not found.");

        guest.FirstName = dto.FirstName ?? guest.FirstName;
        guest.LastName = dto.LastName ?? guest.LastName;
        guest.PhoneNumber = dto.PhoneNumber ?? guest.PhoneNumber;

        var result = await _userManager.UpdateAsync(guest);
        if (!result.Succeeded)
            throw new Exceptions.ValidationException(result.Errors.Select(e => e.Description));
    }

    public async Task DeleteGuestAsync(string guestId, string requesterId, bool isAdmin)
    {
        if (!isAdmin && guestId != requesterId)
            throw new UnauthorizedException("You can only delete your own account.");

        var guest = await _userManager.FindByIdAsync(guestId)
                    ?? throw new NotFoundException($"Guest with id {guestId} not found.");
        var hasActiveReservations = await _userManager.Users
            .AnyAsync(u => u.Id == guestId &&
                           u.GuestReservations.Any(r =>
                               r.CheckOutDate >= DateOnly.FromDateTime(DateTime.UtcNow)));

        if (hasActiveReservations)
            throw new ConflictException("Cannot delete guest with active or future reservations.");

        var result = await _userManager.DeleteAsync(guest);
        if (!result.Succeeded)
            throw new Exceptions.ValidationException(result.Errors.Select(e => e.Description));
    }

    private async Task EnsureRolesExistAsync()
    {
        string[] roles = { "Admin", "Manager", "Guest" };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private async Task CreateUserAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new ArgumentException($"Failed to create user {result.Errors.Select(x => x.Description)}");
    }

    private async Task EnsurePersonalNumberUniqueAsync(string personalNumber)
    {
        var exists = await _userManager.Users
            .AnyAsync(u => u.PersonalNumber == personalNumber);
        if (exists)
            throw new ArgumentException("Personal number is already in use.");
    }

    private async Task EnsurePhoneNumberUniqueAsync(string phoneNumber)
    {
        var exists = await _userManager.Users
            .AnyAsync(u => u.PhoneNumber == phoneNumber);
        if (exists)
            throw new ArgumentException("Phone number is already in use.");
    }

    private async Task EnsureEmailUniqueAsync(string email)
    {
        var exists = await _userManager.FindByEmailAsync(email);
        if (exists != null)
            throw new ArgumentException("Email is already in use.");
    }
}
