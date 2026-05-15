using Microsoft.EntityFrameworkCore;
using CinemaTAPS.API.Models;

namespace CinemaTAPS.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options) 
    { 
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Cinema> Cinemas { get; set; }
    public DbSet<Screening> Screenings { get; set; }
    public DbSet<Reservation> Reservations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().HasIndex(u=>u.Email).IsUnique();

        modelBuilder.Entity<Reservation>().HasIndex(r => new { r.ScreeningId, r.RowPosition, r.SeatPosition }).IsUnique();

        modelBuilder.Entity<Cinema>().HasData(
            new Cinema { Id = 1, Name = "IMAX Auditoria", Rows = 15, SeatsPerRow = 20 },
            new Cinema { Id = 2, Name = "IMAX VIP Auditoria", Rows = 5, SeatsPerRow = 8 },
            new Cinema { Id = 3, Name = "Grand Cinema Auditoria 5D", Rows = 10, SeatsPerRow = 15 },
            new Cinema { Id = 4, Name = "Grand Cinema Hall", Rows = 20, SeatsPerRow = 30 },
            new Cinema { Id = 5, Name = "IMAX Laser Auditoria", Rows = 18, SeatsPerRow = 24 }
        );
        
        modelBuilder.Entity<Screening>().HasData(
            new Screening { Id = 1, MovieTitle = "The Matrix", CinemaId = 1, StartTime = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 2, MovieTitle = "Free Guy", CinemaId = 1, StartTime = new DateTime(2026, 5, 1, 13, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 3, MovieTitle = "The Green Mile", CinemaId = 1, StartTime = new DateTime(2026, 5, 2, 10, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 4, MovieTitle = "Fight Club", CinemaId = 2, StartTime = new DateTime(2026, 5, 2, 18, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 5, MovieTitle = "Total Recall", CinemaId = 2, StartTime = new DateTime(2026, 5, 3, 14, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 6, MovieTitle = "Inception", CinemaId = 4, StartTime = new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 7, MovieTitle = "Interstellar", CinemaId = 5, StartTime = new DateTime(2026, 5, 4, 15, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 8, MovieTitle = "Dune: Part Two", CinemaId = 5, StartTime = new DateTime(2026, 5, 4, 19, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 9, MovieTitle = "Oppenheimer", CinemaId = 4, StartTime = new DateTime(2026, 5, 5, 22, 0, 0, DateTimeKind.Utc) },
            new Screening { Id = 10, MovieTitle = "Spider-Man: Across the Spider-Verse", CinemaId = 3, StartTime = new DateTime(2026, 5, 5, 17, 0, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 9000, 
                FirstName = "Admin",
                LastName = "",
                PhoneNumber = "+0000000000",
                Email = "admin@cinema.com",
                PasswordHash = "admin123",
                Role = "Admin"
            }
        );
    }
}
