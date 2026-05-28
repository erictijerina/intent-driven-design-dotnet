namespace Acme.Application.Dtos.Order;

public sealed class OrderRawDto
{
    public string CustomerReference { get; set; } = string.Empty;

    public string OrderType { get; set; } = string.Empty;

    public List<OrderItemRawDto> Items { get; set; } = new();
}
