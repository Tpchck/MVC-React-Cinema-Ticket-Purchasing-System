using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CinemaTAPS.API.Data;
using CinemaTAPS.API.DTOs;
using CinemaTAPS.API.Services;
using Microsoft.EntityFrameworkCore;

namespace CinemaTAPS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public UsersController(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Reservations)
            .ThenInclude(r => r.Screening)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        return Ok(MapToUserProfileDto(user));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var (success, errorMessage) = await _userService.EditUserProfileAsync(
            userId, 
            request.FirstName, 
            request.LastName, 
            request.PhoneNumber, 
            request.ConcurrencyToken,
            userId,
            false
        );

        if (!success)
        {
            if (errorMessage.Contains("Concurrency"))
                return Conflict(new { message = errorMessage });
            return BadRequest(new { message = errorMessage });
        }

        var user = await _context.Users
            .Include(u => u.Reservations)
            .ThenInclude(r => r.Screening)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return Ok(MapToUserProfileDto(user!));
    }

    private static UserProfileDto MapToUserProfileDto(Models.User user)
    {
        var dto = new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Role = user.Role,
            ConcurrencyToken = user.ConcurrencyToken,
            Reservations = user.Reservations?.Select(r => new ReservationDto
            {
                Id = r.Id,
                RowPosition = r.RowPosition,
                SeatPosition = r.SeatPosition,
                ScreeningId = r.ScreeningId,
                UserId = r.UserId,
                Screening = r.Screening == null ? null : new ScreeningDto
                {
                    Id = r.Screening.Id,
                    MovieTitle = r.Screening.MovieTitle,
                    StartTime = r.Screening.StartTime,
                    CinemaId = r.Screening.CinemaId
                }
            }).ToList() ?? new()
        };

        return dto;
    }
}
