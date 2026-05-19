using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CinemaTAPS.API.Data;
using CinemaTAPS.API.DTOs;
using CinemaTAPS.API.Models;
using CinemaTAPS.API.Services;
using Microsoft.EntityFrameworkCore;

namespace CinemaTAPS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public AdminController(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                Role = u.Role,
                ConcurrencyToken = u.ConcurrencyToken
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserDetail(int id)
    {
        var user = await _context.Users
            .Include(u => u.Reservations)
            .ThenInclude(r => r.Screening)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

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

        return Ok(dto);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        // Read the current admin's ID from the JWT token
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(adminIdClaim, out var currentAdminId))
            return Unauthorized(new { message = "Invalid token." });

        var (success, errorMessage) = await _userService.EditUserProfileAsync(
            id,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.ConcurrencyToken,
            currentAdminId,
            true
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
            .FirstOrDefaultAsync(u => u.Id == id);

        var dto = new UserProfileDto
        {
            Id = user!.Id,
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
                UserId = r.UserId
            }).ToList() ?? new()
        };

        return Ok(dto);
    }

    [HttpPut("reservations/{id}/seat")]
    public async Task<IActionResult> ChangeSeat(int id, [FromBody] ChangeReservationSeatRequest request)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
            return NotFound(new { message = "Reservation not found." });

        // Check if new seat is available
        var existingReservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.ScreeningId == reservation.ScreeningId
                && r.RowPosition == request.RowPosition
                && r.SeatPosition == request.SeatPosition
                && r.Id != id);

        if (existingReservation != null)
            return Conflict(new { message = "Seat is already reserved." });

        reservation.RowPosition = request.RowPosition;
        reservation.SeatPosition = request.SeatPosition;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Seat is no longer available." });
        }

        return Ok(new ReservationDto
        {
            Id = reservation.Id,
            RowPosition = reservation.RowPosition,
            SeatPosition = reservation.SeatPosition,
            ScreeningId = reservation.ScreeningId,
            UserId = reservation.UserId
        });
    }

    [HttpDelete("reservations/{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
            return NotFound(new { message = "Reservation not found." });

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
