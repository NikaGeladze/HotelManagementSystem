using HMS.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HMS.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options
    ) : base(options)
    {
    }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationRoom> ReservationRooms { get; set; }
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
         // ── Hotel ──────────────────────────────────────────
        builder.Entity<Hotel>()
            .HasMany(h => h.Managers)
            .WithOne(u => u.ManagedHotel)
            .HasForeignKey(u => u.HotelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Hotel>()
            .HasMany(h => h.Rooms)
            .WithOne(r => r.Hotel)
            .HasForeignKey(r => r.HotelId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // ── ApplicationUser ────────────────────────────────
        builder.Entity<ApplicationUser>()
            .Property(u => u.PersonalNumber)
            .IsFixedLength();

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.PersonalNumber)
            .IsUnique();

        // ── Room ───────────────────────────────────────────
        builder.Entity<Room>()
            .Property(r => r.Price)
            .HasColumnType("decimal(18,2)");

        // ── Reservation ────────────────────────────────────
        

        builder.Entity<Reservation>()
            .HasOne(r => r.Guest)        
            .WithMany(u => u.GuestReservations)
            .HasForeignKey(r => r.GuestId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // ── ReservationRoom ────────────────────────────────
        builder.Entity<ReservationRoom>()
            .HasKey(rr => new { rr.ReservationId, rr.RoomId });

        builder.Entity<ReservationRoom>()
            .HasOne(rr => rr.Reservation)
            .WithMany(r => r.ReservationRooms)
            .HasForeignKey(rr => rr.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ReservationRoom>()
            .HasOne(rr => rr.Room)
            .WithMany(r => r.ReservationRooms)
            .HasForeignKey(rr => rr.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}