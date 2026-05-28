using Acme.Domain.Order;
using Xunit;

namespace Domain.Tests.Order;

public sealed class OrderTests
{
    [Fact]
    public void IsEligibleForProcessing_WhenPending_ReturnsTrue()
    {
        var order = new global::Acme.Domain.Order.Order { Status = OrderStatus.Pending };
        Assert.True(order.IsEligibleForProcessing());
    }
}
