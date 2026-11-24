using Microsoft.EntityFrameworkCore;
using Salon.Booking;
using Salon.Shops;

namespace Salon.Data;

public class SalonDbContext(DbContextOptions<SalonDbContext> options) : DbContext(options)
{
    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<TimeOff> TimeOff => Set<TimeOff>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public static void Configure(DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(connectionString, npgsql => npgsql.MapEnum<AppointmentStatus>("appointment_status"))
            .UseSnakeCaseNamingConvention();

    protected override void ConfigureConventions(ModelConfigurationBuilder configuration)
    {
        configuration.Properties<DateTime>().HavePrecision(3);
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Shop>(shop =>
        {
            shop.Property(s => s.SlotIntervalMin).HasDefaultValue(15);
            shop.HasMany(s => s.Staff).WithOne().HasForeignKey(s => s.ShopId);
            shop.HasMany(s => s.Services).WithOne().HasForeignKey(s => s.ShopId);
        });

        model.Entity<Staff>()
            .HasMany(s => s.WorkingHours).WithOne().HasForeignKey(h => h.StaffId);

        model.Entity<WorkingHours>()
            .HasIndex(h => new { h.StaffId, h.Weekday }).IsUnique();

        model.Entity<TimeOff>(timeOff =>
        {
            timeOff.HasOne<Staff>().WithMany().HasForeignKey(t => t.StaffId);
            timeOff.HasIndex(t => new { t.StaffId, t.StartAt });
        });

        model.Entity<Appointment>(appointment =>
        {
            appointment.HasOne<Shop>().WithMany().HasForeignKey(a => a.ShopId);
            appointment.HasOne<Staff>().WithMany().HasForeignKey(a => a.StaffId);
            appointment.HasOne(a => a.Service).WithMany().HasForeignKey(a => a.ServiceId);
            appointment.Property(a => a.CreatedAt).HasDefaultValueSql("now()");
            appointment.Property(a => a.UpdatedAt).HasDefaultValueSql("now()");
            appointment.HasIndex(a => new { a.StaffId, a.StartAt });
            appointment.HasIndex(a => new { a.ShopId, a.StartAt });
        });

        foreach (var foreignKey in model.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
