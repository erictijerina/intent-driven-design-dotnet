using Acme.Application.Dtos.Order;
using Acme.Application.Dtos.OrderHistory;
using Acme.Domain.Order;
using Acme.Domain.Order.Commands;
using AutoMapper;

namespace Acme.Application.Mappings;

public sealed class Profile : AutoMapper.Profile
{
    public Profile()
    {
        CreateMap<OrderItemRawDto, PlaceOrderItem>();
        CreateMap<OrderRawDto, PlaceOrder>();
        CreateMap<Order, OrderResponse>()
            .ForMember(x => x.Status, c => c.MapFrom(s => s.Status.ToString()));
        CreateMap<global::Acme.Domain.OrderHistory.OrderHistory, OrderHistoryResponse>();
    }
}
