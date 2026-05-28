using Acme.Application.Contracts;
using Acme.Domain.Order;
using Microsoft.EntityFrameworkCore;

namespace Acme.Infrastructure.Database.Sql;

public sealed class OrderStore(AcmeDbContext dbContext) : IOrderStore
{
    public Task AddAsync(Order order, CancellationToken ct = default)
        => dbContext.Orders.AddAsync(order, ct).AsTask();

    public Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        => dbContext.Orders.FirstOrDefaultAsync(x => x.Id == orderId, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => dbContext.SaveChangesAsync(ct);
}
