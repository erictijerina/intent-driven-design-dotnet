namespace Acme.Application.Dtos.Order;

public sealed class OrderResponse
{
    public Guid Id { get; set; }

    public string CustomerReference { get; set; } = string.Empty;

    public string OrderType { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
