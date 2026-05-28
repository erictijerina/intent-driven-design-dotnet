using Acme.Domain.Order;

namespace Acme.Application.Contracts;

public interface IOrderStore
{
    Task AddAsync(Order order, CancellationToken ct = default);

    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
