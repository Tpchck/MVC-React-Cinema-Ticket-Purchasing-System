namespace CinemaTAPS.API.Models;
using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public List<Reservation> Reservations { get; set; } = new();
    [ConcurrencyCheck]
    public Guid ConcurrencyToken { get; set; }
}
