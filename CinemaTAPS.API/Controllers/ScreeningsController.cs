using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CinemaTAPS.API.Data;
using CinemaTAPS.API.Models;
using CinemaTAPS.API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CinemaTAPS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ScreeningsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var screenings = await _context.Screenings
            .Include(s => s.Cinema)
            .Select(s => MapToScreeningDto(s))
            .ToListAsync();

        return Ok(screenings);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateScreeningRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.MovieTitle))
            return BadRequest(new { message = "Movie title is required." });

        var cinema = await _context.Cinemas.FindAsync(request.CinemaId);
        if (cinema == null)
            return NotFound(new { message = "Cinema not found." });

        var screening = new Screening
        {
            MovieTitle = request.MovieTitle,
            StartTime = request.StartTime,
            CinemaId = request.CinemaId
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        var dto = MapToScreeningDto(screening);
        return CreatedAtAction(nameof(GetById), new { id = screening.Id }, dto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var screening = await _context.Screenings
            .Include(s => s.Cinema)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (screening == null)
            return NotFound(new { message = "Screening not found." });

        return Ok(MapToScreeningDto(screening));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var screening = await _context.Screenings.FindAsync(id);
        if (screening == null)
            return NotFound(new { message = "Screening not found." });

        _context.Screenings.Remove(screening);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ScreeningDto MapToScreeningDto(Screening screening)
    {
        return new ScreeningDto
        {
            Id = screening.Id,
            MovieTitle = screening.MovieTitle,
            StartTime = screening.StartTime,
            CinemaId = screening.CinemaId,
            Cinema = screening.Cinema == null ? null : new CinemaDto
            {
                Id = screening.Cinema.Id,
                Name = screening.Cinema.Name,
                Rows = screening.Cinema.Rows,
                SeatsPerRow = screening.Cinema.SeatsPerRow
            }
        };
    }
}
