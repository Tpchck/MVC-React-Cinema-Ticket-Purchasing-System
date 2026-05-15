using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CinemaTAPS.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cinemas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rows = table.Column<int>(type: "integer", nullable: false),
                    SeatsPerRow = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cinemas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyToken = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Screenings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MovieTitle = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CinemaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Screenings_Cinemas_CinemaId",
                        column: x => x.CinemaId,
                        principalTable: "Cinemas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RowPosition = table.Column<int>(type: "integer", nullable: false),
                    SeatPosition = table.Column<int>(type: "integer", nullable: false),
                    ScreeningId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Screenings_ScreeningId",
                        column: x => x.ScreeningId,
                        principalTable: "Screenings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Cinemas",
                columns: new[] { "Id", "Name", "Rows", "SeatsPerRow" },
                values: new object[,]
                {
                    { 1, "IMAX Auditoria", 15, 20 },
                    { 2, "IMAX VIP Auditoria", 5, 8 },
                    { 3, "Grand Cinema Auditoria 5D", 10, 15 },
                    { 4, "Grand Cinema Hall", 20, 30 },
                    { 5, "IMAX Laser Auditoria", 18, 24 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "ConcurrencyToken", "Email", "FirstName", "LastName", "PasswordHash", "PhoneNumber", "Role" },
                values: new object[] { 9000, new Guid("00000000-0000-0000-0000-000000000000"), "admin@cinema.com", "Admin", "", "admin123", "+0000000000", "Admin" });

            migrationBuilder.InsertData(
                table: "Screenings",
                columns: new[] { "Id", "CinemaId", "MovieTitle", "StartTime" },
                values: new object[,]
                {
                    { 1, 1, "The Matrix", new DateTime(2026, 5, 1, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, "Free Guy", new DateTime(2026, 5, 1, 13, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, "The Green Mile", new DateTime(2026, 5, 2, 10, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 2, "Fight Club", new DateTime(2026, 5, 2, 18, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 2, "Total Recall", new DateTime(2026, 5, 3, 14, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 4, "Inception", new DateTime(2026, 5, 3, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 5, "Interstellar", new DateTime(2026, 5, 4, 15, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 5, "Dune: Part Two", new DateTime(2026, 5, 4, 19, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 4, "Oppenheimer", new DateTime(2026, 5, 5, 22, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, 3, "Spider-Man: Across the Spider-Verse", new DateTime(2026, 5, 5, 17, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ScreeningId_RowPosition_SeatPosition",
                table: "Reservations",
                columns: new[] { "ScreeningId", "RowPosition", "SeatPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UserId",
                table: "Reservations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenings_CinemaId",
                table: "Screenings",
                column: "CinemaId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Screenings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cinemas");
        }
    }
}
