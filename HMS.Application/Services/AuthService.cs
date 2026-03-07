using System.ComponentModel.DataAnnotations;
using HMS.Application.Interfaces.Services;
using HMS.Application.Models.DTOs.Auth;
using HMS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HMS.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
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
