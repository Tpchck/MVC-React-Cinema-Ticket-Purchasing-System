namespace CinemaTAPS.API.DTOs;

public class SeatDto
{
    public int Row { get; set; }
    public int Position { get; set; }
    public string Status { get; set; } = string.Empty; // Free, Occupied, YourSeat
    public int? ReservationId { get; set; }
    public int? ReservedByUserId { get; set; }
}

public class SeatMapDto
{
    public int ScreeningId { get; set; }
    public ScreeningDto? Screening { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public List<SeatDto> Seats { get; set; } = new();
}

public class ReservationDto
{
    public int Id { get; set; }
    public int RowPosition { get; set; }
    public int SeatPosition { get; set; }
    public int ScreeningId { get; set; }
    public int UserId { get; set; }
    public ScreeningDto? Screening { get; set; }
}

public class BookSeatRequest
{
    public int ScreeningId { get; set; }
    public int RowPosition { get; set; }
    public int SeatPosition { get; set; }
}
