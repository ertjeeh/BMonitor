using BMonitor.DAL.Models;
using BMonitor.DAL.Models.Cards;
using BMonitor.DAL.Models.Monitors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BMonitor.DAL;

public class BMonitorContext : DbContext
{
    private readonly ILogger<BMonitorContext> _logger;

    public DbSet<Setting> Settings { get; set; }
    public DbSet<Instance> Instances { get; set; }
    public DbSet<Models.Monitor> Monitors { get; set; }
    public DbSet<PingMonitor> PingMonitors { get; set; }
    public DbSet<FolderMonitor> FolderMonitors { get; set; }
    public DbSet<HttpMonitor> HttpMonitors { get; set; }
    public DbSet<SqlMonitor> SqlMonitors { get; set; }

    public DbSet<Card> Cards { get; set; }
    public DbSet<HtmlCard> HtmlCards { get; set; }

    public DbSet<StatusResult> StatusResults { get; set; }
    public DbSet<MonitorResult> MonitorResults { get; set; }

    public BMonitorContext()
    {
    }

    public BMonitorContext(ILogger<BMonitorContext> logger, DbContextOptions<BMonitorContext> options) : base(options)
    {
        _logger = logger;
        var pendingMigrations = Database.GetPendingMigrations().Count();
        if (pendingMigrations <= 0)
        {
            return;
        }

        _logger.LogInformation($"Target database out of date, need to apply {pendingMigrations} migrations. Migrating now...");
        Database.Migrate();
        _logger.LogInformation("Migrations completed.");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PingMonitor>().ToTable("PingMonitors");
        modelBuilder.Entity<FolderMonitor>().ToTable("FolderMonitors");
        modelBuilder.Entity<HttpMonitor>().ToTable("HttpMonitors");
        modelBuilder.Entity<SqlMonitor>().ToTable("SqlMonitors");

        modelBuilder.Entity<HtmlCard>().ToTable("HtmlCards");

        modelBuilder.Entity<Instance>()
            .HasMany(i => i.Cards)
            .WithMany(c => c.Instances);

        modelBuilder.Entity<MonitorResult>()
            .HasOne(mr => mr.Monitor)
            .WithMany(m => m.MonitorResults)
            .HasForeignKey(mr => mr.MonitorId);

        modelBuilder.Entity<MonitorResult>()
            .Property(e => e.StatusResultId)
            .HasConversion<int>();

        modelBuilder.Entity<StatusResult>()
            .Property(e => e.StatusResultId)
            .HasConversion<int>();

        modelBuilder.Entity<StatusResult>().HasData(
            Enum.GetValues(typeof(StatusResultId))
                .Cast<StatusResultId>()
                .Select(e => new StatusResult
                {
                    StatusResultId = e,
                    Name = e.ToString()
                }));
    }
}