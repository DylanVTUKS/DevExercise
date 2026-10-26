using IOT_Devices.Models;
using Microsoft.EntityFrameworkCore;

namespace IOT_Devices.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DeviceType> DeviceTypes => Set<DeviceType>();
    public DbSet<Firmware> Firmware => Set<Firmware>();
    public DbSet<DeviceGroup> DeviceGroups => Set<DeviceGroup>();
    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<DeviceType>().ToTable("DeviceType");
        modelBuilder.Entity<Firmware>().ToTable("Firmware");
        modelBuilder.Entity<DeviceGroup>().ToTable("DeviceGroup");
        modelBuilder.Entity<Device>().ToTable("Device");


        //g = group, d = device. 
        modelBuilder.Entity<DeviceGroup>()
            .HasOne(g => g.Parent)
            .WithMany(g => g.Children)
            .HasForeignKey(g => g.ParentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Firmware>()
            .HasOne(f => f.DeviceType)
            .WithMany(t => t.Firmware)
            .HasForeignKey(f => f.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Device>()
            .HasOne(d => d.DeviceType)
            .WithMany(t => t.Devices)
            .HasForeignKey(d => d.DeviceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Device>()
            .HasOne(d => d.Firmware)
            .WithMany(f => f.Devices)
            .HasForeignKey(d => d.FirmwareId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Device>()
            .HasOne(d => d.Group)
            .WithMany(g => g.Devices)
            .HasForeignKey(d => d.GroupId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}