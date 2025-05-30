using CheckoutSystem.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CheckoutSystem.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly ConcurrentDictionary<string, ShoppingCart> _baskets = new();
        private readonly ILogger<BasketRepository> _logger;

        public BasketRepository(ILogger<BasketRepository> logger)
        {
            _logger = logger;
        }

        public Task<ShoppingCart?> GetBasket(string userName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting basket for user: {UserName}", userName);

            if (_baskets.TryGetValue(userName, out var basket))
            {
                Task.FromResult<ShoppingCart?>(basket);
            }

            return Task.FromResult<ShoppingCart?>(null);
        }

        public Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Storing basket for user: {UserName}", basket.UserName);

            _baskets[basket.UserName] = basket;

            return Task.FromResult(basket);
        }

        public Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting basket for user: {UserName}", userName);

            var result = _baskets.TryRemove(userName, out _);

            return Task.FromResult(result);
        }
    }
}
