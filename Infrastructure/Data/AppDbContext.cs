using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<View> Views => Set<View>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<BillingReport> BillingReports => Set<BillingReport>();
    public DbSet<BillingOperation> BillingOperations => Set<BillingOperation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurateReport(modelBuilder);
        ConfigurateOutboxMessage(modelBuilder);
        ConfigurateView(modelBuilder);
        ConfigurateBillingReport(modelBuilder);
        ConfigurateBillingOperation(modelBuilder);
    }

    private static void ConfigurateView(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<View>(entity =>
        {
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
    
    private static void ConfigurateOutboxMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(o => o.EventType).IsRequired().HasMaxLength(255);
            entity.Property(o => o.Payload).IsRequired().HasColumnType("text");
        });
    }
    
    private static void ConfigurateReport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Status).HasConversion<string>();
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(r => r.DecorationId).IsRequired().HasMaxLength(20);
        });
    }

    private static void ConfigurateBillingReport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingReport>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.UserId).IsRequired().HasMaxLength(255);
            entity.Property(r => r.LastPaymentTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
    
    private static void ConfigurateBillingOperation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingOperation>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.UserId).IsRequired().HasMaxLength(255);
            entity.Property(o => o.OperationType).IsRequired().HasMaxLength(50);
            entity.Property(o => o.OperationTime).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(o => o.Amount).IsRequired().HasColumnType("decimal(10,2)");
        });
    }
}