using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CinemaTAPS.API.Data;
using CinemaTAPS.API.DTOs;
using CinemaTAPS.API.Models;
using CinemaTAPS.API.Services;

namespace CinemaTAPS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        var existingUser = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
            return Conflict(new { message = "User with this email already exists." });

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            PasswordHash = request.Password,
            Role = "User",
            ConcurrencyToken = Guid.NewGuid()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var userDto = MapToUserDto(user);
        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = userDto
        });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null || user.PasswordHash != request.Password)
            return Unauthorized(new { message = "Email or password is incorrect." });

        var userDto = MapToUserDto(user);
        var token = _jwtService.GenerateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = userDto
        });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid token." });

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return NotFound(new { message = "User not found." });

        var userDto = MapToUserDto(user);
        return Ok(userDto);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Role = user.Role,
            ConcurrencyToken = user.ConcurrencyToken
        };
    }
}
