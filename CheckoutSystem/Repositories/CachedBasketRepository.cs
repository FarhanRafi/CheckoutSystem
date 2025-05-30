using CheckoutSystem.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CheckoutSystem.Repositories
{
    public class CachedBasketRepository : IBasketRepository
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachedBasketRepository> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public CachedBasketRepository(
            IBasketRepository basketRepository,
            IMemoryCache cache,
            ILogger<CachedBasketRepository> logger)
        {
            _basketRepository = basketRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ShoppingCart?> GetBasket(string userName, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"basket_{userName}";

            if (_cache.TryGetValue(cacheKey, out ShoppingCart? cachedBasket))
            {
                _logger.LogInformation("Cache hit for basket: {UserName}", userName);
                return cachedBasket;
            }

            _logger.LogInformation("Cache miss for basket: {UserName}", userName);
            var basket = await _basketRepository.GetBasket(userName, cancellationToken);

            if (basket != null)
            {
                _cache.Set(cacheKey, basket, _cacheExpiration);
            }

            return basket;
        }

        public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"basket_{basket.UserName}";

            var storedBasket = await _basketRepository.StoreBasket(basket, cancellationToken);

            _cache.Set(cacheKey, storedBasket, _cacheExpiration);
            _logger.LogInformation("Updated cache for basket: {UserName}", basket.UserName);
            
            return storedBasket;
        }

        public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"basket_{userName}";

            var result = await _basketRepository.DeleteBasket(userName, cancellationToken);

            if (result)
            {
                _cache.Remove(cacheKey);
                _logger.LogInformation("Removed from cache: {UserName}", userName);
            }

            return result;
        }
    }
}
