namespace CinemaTAPS.API.DTOs;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public Guid ConcurrencyToken { get; set; }
}

public class UserProfileDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid ConcurrencyToken { get; set; }
    public List<ReservationDto> Reservations { get; set; } = new();
}

public class ChangeReservationSeatRequest
{
    public int RowPosition { get; set; }
    public int SeatPosition { get; set; }
}
