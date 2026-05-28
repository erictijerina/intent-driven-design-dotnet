using Acme.Domain.OrderHistory.Commands;
using Acme.Infrastructure.Database.Mongo.OrderHistory;
using MediatR;
using MongoDB.Driver;

namespace Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Commands;

public sealed class RecordOrderCompletedHandler : IRequestHandler<RecordOrderCompleted, Unit>
{
    private readonly IMongoCollection<OrderHistoryDocument> _collection;

    public RecordOrderCompletedHandler(IMongoDatabase database)
    {
        _collection = database.GetCollection<OrderHistoryDocument>("order_history");
    }

    public async Task<Unit> Handle(RecordOrderCompleted request, CancellationToken ct)
    {
        var orderId = request.OrderId.ToString("D");
        var filter = Builders<OrderHistoryDocument>.Filter.Eq(x => x.OrderId, orderId);
        var update = Builders<OrderHistoryDocument>.Update
            .SetOnInsert(x => x.OrderId, orderId)
            .Set(x => x.OrderCompletedAt, request.OccurredAt);

        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
        return Unit.Value;
    }
}
