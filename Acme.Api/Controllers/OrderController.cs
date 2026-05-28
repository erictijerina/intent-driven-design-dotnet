using Acme.Application.Dtos.Order;
using Acme.Domain.Order.Commands;
using Acme.Domain.Order.Queries;
using Acme.Domain.Order.Workflows;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrderController : ControllerBase
{
    private readonly IMapper _mapper;

    public OrderController(IMapper mapper)
    {
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] OrderRawDto request, CancellationToken ct)
    {
        var intent = _mapper.Map<PlaceOrder>(request);
        var result = await global::Acme.Domain.Order.Order.PlaceOrder(intent, ct);
        return Ok(_mapper.Map<OrderResponse>(result));
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken ct)
    {
        var result = await global::Acme.Domain.Order.Order.GetOrder(new GetOrder(orderId), ct);
        return result is null ? NotFound() : Ok(_mapper.Map<OrderResponse>(result));
    }

    [HttpPost("{orderId:guid}/process")]
    public async Task<IActionResult> ProcessOrder(Guid orderId, CancellationToken ct)
    {
        var result = await global::Acme.Domain.Order.Order.ProcessOrder(new ProcessOrder(orderId), ct);
        return Ok(result);
    }
}
