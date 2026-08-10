using Microsoft.AspNetCore.Mvc;
using OrderRefactor.Models;
using OrderRefactor.Services;

namespace OrderRefactor.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<ActionResult<OrderResult>> CreateOrder(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _orderService.CreateOrderAsync(
                request,
                cancellationToken);

            return Created(
                $"/api/orders/{result.OrderId}",
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                error = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                error = ex.Message
            });
        }
    }
}