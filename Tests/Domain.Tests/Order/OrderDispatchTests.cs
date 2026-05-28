using Acme.Domain.Order.Commands;
using Acme.Domain.Order.Events;
using Domain.Tests.Fixtures;
using Xunit;

namespace Domain.Tests.Order;

public sealed class OrderDispatchTests
{
    [Fact]
    public async Task PlaceOrder_DispatchesIntent()
    {
        var expected = new global::Acme.Domain.Order.Order { CustomerReference = "cust" };

        DomainDispatchFixture.Configure(
            new FakeIntentRuntime(_ => expected),
            new FakeEventRuntime(_ => { }));

        try
        {
            var result = await global::Acme.Domain.Order.Order.PlaceOrder(
                new PlaceOrder("cust", "standard", new[] { new PlaceOrderItem("sku", 1) }));

            Assert.Same(expected, result);
        }
        finally
        {
            DomainDispatchFixture.Reset();
        }
    }

    [Fact]
    public async Task OrderPlaced_RaisesEvent()
    {
        OrderPlaced? raised = null;

        DomainDispatchFixture.Configure(
            new FakeIntentRuntime(_ => null),
            new FakeEventRuntime(evt => raised = (OrderPlaced)evt));

        try
        {
            var evt = new OrderPlaced(Guid.NewGuid(), "cust", "standard", global::Acme.Domain.Order.OrderStatus.Pending, DateTimeOffset.UtcNow);
            await global::Acme.Domain.Order.Order.OrderPlaced(evt);

            Assert.NotNull(raised);
            Assert.Equal(evt.OrderId, raised!.OrderId);
        }
        finally
        {
            DomainDispatchFixture.Reset();
        }
    }
}
