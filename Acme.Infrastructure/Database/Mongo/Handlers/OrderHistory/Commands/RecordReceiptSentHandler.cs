using Acme.Domain.OrderHistory.Commands;
using Acme.Infrastructure.Database.Mongo.OrderHistory;
using MediatR;
using MongoDB.Driver;

namespace Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Commands;

public sealed class RecordReceiptSentHandler : IRequestHandler<RecordReceiptSent, Unit>
{
    private readonly IMongoCollection<OrderHistoryDocument> _collection;

    public RecordReceiptSentHandler(IMongoDatabase database)
    {
        _collection = database.GetCollection<OrderHistoryDocument>("order_history");
    }

    public async Task<Unit> Handle(RecordReceiptSent request, CancellationToken ct)
    {
        var orderId = request.OrderId.ToString("D");
        var filter = Builders<OrderHistoryDocument>.Filter.Eq(x => x.OrderId, orderId);
        var update = Builders<OrderHistoryDocument>.Update
            .SetOnInsert(x => x.OrderId, orderId)
            .Set(x => x.ReceiptSentAt, request.OccurredAt);

        await _collection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, ct);
        return Unit.Value;
    }
}
