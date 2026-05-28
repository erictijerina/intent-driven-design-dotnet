using Acme.Domain.Order;
using Acme.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Acme.Infrastructure.Database.Sql;

public sealed class AcmeDbContext(DbContextOptions<AcmeDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CustomerReference).HasMaxLength(128).IsRequired();
            entity.Property(x => x.OrderType).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasMaxLength(256).IsRequired();
            entity.Property(x => x.Payload).IsRequired();
        });
    }
}
