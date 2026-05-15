namespace CinemaTAPS.API.Models;

public class Reservation
{
    public int Id { get; set; }
    public int RowPosition { get; set; }
    public int SeatPosition { get; set; }
    public int ScreeningId { get; set; }
    public Screening? Screening { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
