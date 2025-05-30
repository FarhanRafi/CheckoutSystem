using CheckoutSystem.Models;
using CheckoutSystem.Repositories;
using CheckoutSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CheckoutSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly CheckoutService _checkoutService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            IBasketRepository basketRepository,
            CheckoutService checkoutService,
            ILogger<CartController> logger)
        {
            _basketRepository = basketRepository;
            _checkoutService = checkoutService;
            _logger = logger;
        }

        [HttpPost("store")]
        public async Task<IActionResult> StoreCart([FromBody] StoreCartRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId) || request.Items == null || request.Items.Count == 0)
            {
                return BadRequest("UserId and Items are required");
            }

            var cart = new ShoppingCart
            {
                UserName = request.UserId,
                Items = request.Items
            };

            await _basketRepository.StoreBasket(cart, cancellationToken);

            return Ok(new { Message = "Cart stored successfully" });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteCart(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            var result = await _basketRepository.DeleteBasket(userId, cancellationToken);

            if (!result)
            {
                return NotFound($"User not found or cart doesn't exist");
            }

            return Ok(new { Message = "Cart successfully cleared" });
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest("UserId is required");
            }

            var result = await _checkoutService.ProcessCheckoutAsync(request.UserId, cancellationToken);

            if (!result.Success)
            {
                if (result.Message.Contains("Too many concurrent checkouts"))
                {
                    return StatusCode(429, new { Message = result.Message }); // 429 Too Many Requests
                }

                return BadRequest(new { Message = result.Message });
            }

            return Ok(new {
                Message = "Checkout completed successfully",
                Items = result.Items,
                CheckoutTime = result.CheckoutTime
            });
        }
    }

    public class StoreCartRequest
    {
        public required string UserId { get; set; }
        public required List<string> Items { get; set; } = [];
    }

    public class CheckoutRequest
    {
        public required string UserId { get; set; }
    }
}
