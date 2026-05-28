using Acme.Domain.OrderHistory.Queries;
using Acme.Infrastructure.Database.Mongo.OrderHistory;
using MediatR;
using MongoDB.Driver;

namespace Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Queries;

public sealed class GetOrderHistoryHandler : IRequestHandler<GetOrderHistory, global::Acme.Domain.OrderHistory.OrderHistory?>
{
    private readonly IMongoCollection<OrderHistoryDocument> _collection;

    public GetOrderHistoryHandler(IMongoDatabase database)
    {
        _collection = database.GetCollection<OrderHistoryDocument>("order_history");
    }

    public async Task<global::Acme.Domain.OrderHistory.OrderHistory?> Handle(GetOrderHistory request, CancellationToken ct)
    {
        var id = request.OrderId.ToString("D");
        var document = await _collection.Find(x => x.OrderId == id).FirstOrDefaultAsync(ct);
        if (document is null)
            return null;

        return global::Acme.Domain.OrderHistory.OrderHistory.FromSnapshot(
            request.OrderId,
            document.OrderReceivedAt,
            document.OrderProcessedAt,
            document.ReceiptSentAt,
            document.OrderCompletedAt);
    }
}
