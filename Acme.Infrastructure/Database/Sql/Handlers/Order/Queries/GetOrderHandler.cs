using Acme.Domain.Order.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Acme.Infrastructure.Database.Sql.Handlers.Order.Queries;

public sealed class GetOrderHandler(AcmeDbContext dbContext) : IRequestHandler<GetOrder, global::Acme.Domain.Order.Order?>
{
    public Task<global::Acme.Domain.Order.Order?> Handle(GetOrder request, CancellationToken ct)
        => dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, ct);
}
