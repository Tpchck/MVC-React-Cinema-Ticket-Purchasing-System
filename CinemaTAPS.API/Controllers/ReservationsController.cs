using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CinemaTAPS.API.Data;
using CinemaTAPS.API.Models;
using CinemaTAPS.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CinemaTAPS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReservationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("seatmap/{screeningId}")]
    public async Task<IActionResult> GetSeatMap(int screeningId)
    {
        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == screeningId);

        if (screening == null || screening.Cinema == null)
            return NotFound(new { message = "Screening not found." });

        var reservations = await _context.Reservations
            .Where(r => r.ScreeningId == screeningId)
            .ToListAsync();

        var seats = new List<SeatDto>();
        for (int row = 1; row <= screening.Cinema.Rows; row++)
        {
            for (int pos = 1; pos <= screening.Cinema.SeatsPerRow; pos++)
            {
                var reservation = reservations.FirstOrDefault(r => r.RowPosition == row && r.SeatPosition == pos);
                
                var seatDto = new SeatDto
                {
                    Row = row,
                    Position = pos,
                    Status = reservation == null ? "Free" : "Occupied",
                    ReservationId = reservation?.Id,
                    ReservedByUserId = reservation?.UserId
                };

                seats.Add(seatDto);
            }
        }

        var screeningDto = new ScreeningDto
        {
            Id = screening.Id,
            MovieTitle = screening.MovieTitle,
            StartTime = screening.StartTime,
            CinemaId = screening.CinemaId,
            Cinema = new CinemaDto
            {
                Id = screening.Cinema.Id,
                Name = screening.Cinema.Name,
                Rows = screening.Cinema.Rows,
                SeatsPerRow = screening.Cinema.SeatsPerRow
            }
        };

        var seatMap = new SeatMapDto
        {
            ScreeningId = screeningId,
            Screening = screeningDto,
            Rows = screening.Cinema.Rows,
            SeatsPerRow = screening.Cinema.SeatsPerRow,
            Seats = seats
        };

        return Ok(seatMap);
    }

    [HttpPost("book")]
    [Authorize]
    public async Task<IActionResult> BookSeat([FromBody] BookSeatRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid token." });

        var screening = await _context.Screenings.FindAsync(request.ScreeningId);
        if (screening == null)
            return NotFound(new { message = "Screening not found." });

        // Check if seat is already reserved
        var existingReservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.ScreeningId == request.ScreeningId 
                && r.RowPosition == request.RowPosition 
                && r.SeatPosition == request.SeatPosition);

        if (existingReservation != null)
            return Conflict(new { message = "Seat is already reserved." });

        var reservation = new Reservation
        {
            ScreeningId = request.ScreeningId,
            RowPosition = request.RowPosition,
            SeatPosition = request.SeatPosition,
            UserId = userId
        };

        _context.Reservations.Add(reservation);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Concurrency conflict - seat was taken by another user
            if (ex.InnerException?.Message.Contains("unique") == true)
                return Conflict(new { message = "Seat is no longer available." });
            throw;
        }

        var dto = MapToReservationDto(reservation);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, dto);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Invalid token." });

        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
            return NotFound(new { message = "Reservation not found." });

        var user = await _context.Users.FindAsync(userId);
        var isAdmin = user?.Role == "Admin";

        if (reservation.UserId != userId && !isAdmin)
            return Forbid();

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var reservation = await _context.Reservations
            .Include(r => r.Screening)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
            return NotFound();

        return Ok(MapToReservationDto(reservation));
    }

    private static ReservationDto MapToReservationDto(Reservation reservation)
    {
        var dto = new ReservationDto
        {
            Id = reservation.Id,
            RowPosition = reservation.RowPosition,
            SeatPosition = reservation.SeatPosition,
            ScreeningId = reservation.ScreeningId,
            UserId = reservation.UserId
        };

        if (reservation.Screening != null)
        {
            dto.Screening = new ScreeningDto
            {
                Id = reservation.Screening.Id,
                MovieTitle = reservation.Screening.MovieTitle,
                StartTime = reservation.Screening.StartTime,
                CinemaId = reservation.Screening.CinemaId
            };
        }

        return dto;
    }
}
