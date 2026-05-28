namespace Acme.Application.Dtos.Order;

public sealed class OrderItemRawDto
{
    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }
}
