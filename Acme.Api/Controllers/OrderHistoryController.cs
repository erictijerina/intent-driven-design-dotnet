using Acme.Application.Dtos.OrderHistory;
using Acme.Domain.OrderHistory.Queries;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrderHistoryController : ControllerBase
{
    private readonly IMapper _mapper;

    public OrderHistoryController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrderHistory(Guid orderId, CancellationToken ct)
    {
        var result = await global::Acme.Domain.OrderHistory.OrderHistory.GetOrderHistory(
            new GetOrderHistory(orderId), ct);
        return result is null ? NotFound() : Ok(_mapper.Map<OrderHistoryResponse>(result));
    }
}
