namespace CinemaTAPS.API.Models;

public class Screening
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int CinemaId { get; set; }
    public Cinema? Cinema { get; set; }
    public List<Reservation> Reservations { get; set; } = new();
}
