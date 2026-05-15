namespace CinemaTAPS.API.DTOs;

public class CinemaDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}

public class ScreeningDto
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int CinemaId { get; set; }
    public CinemaDto? Cinema { get; set; }
}

public class CreateScreeningRequest
{
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int CinemaId { get; set; }
}
