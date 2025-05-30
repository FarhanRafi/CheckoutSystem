using CheckoutSystem.Models;
using CheckoutSystem.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CheckoutSystem.Services
{
    public class CheckoutService
    {
        private readonly IBasketRepository _basketRepository;
        private readonly ILogger<CheckoutService> _logger;

        // Semaphore to limit concurrent checkouts to 3
        private readonly SemaphoreSlim _checkoutSemaphore = new SemaphoreSlim(3, 3);

        public CheckoutService(IBasketRepository basketRepository, ILogger<CheckoutService> logger)
        {
            _basketRepository = basketRepository;
            _logger = logger;
        }

        public async Task<CheckoutResult> ProcessCheckoutAsync(string userName, CancellationToken cancellationToken = default)
        {
            // try entering semaphore without waiting
            bool acquired = _checkoutSemaphore.Wait(0, cancellationToken);

            if (!acquired)
            {
                _logger.LogWarning("Too many concurrent checkouts. Rejecting checkout for user: {UserName}", userName);
                return new CheckoutResult
                {
                    Success = false,
                    Message = "Too Many Checkout Requests. Please try again later."
                };
            }

            try
            {
                _logger.LogInformation("Processing checkout for user: {UserName}", userName);

                var basket = await _basketRepository.GetBasket(userName, cancellationToken);

                if (basket == null || basket.Items.Count == 0)
                {
                    return new CheckoutResult
                    {
                        Success = false,
                        Message = "Cart is empty or does not exist."
                    };
                }

                // simulate checkout processing time 3000ms
                _logger.LogInformation("Processing order for userId: {UserName}", userName);
                await Task.Delay(3000, cancellationToken);

                await _basketRepository.DeleteBasket(userName, cancellationToken);

                _logger.LogInformation("Order processed for userId: {UserName}", userName);

                // simulate sending a confirmation email
                _logger.LogInformation("Sending confirmation email to userId: {UserName}", userName);

                return new CheckoutResult
                {
                    Success = true,
                    Message = "Checkout completed successfully.",
                    Items = basket.Items,
                    CheckoutTime = DateTime.UtcNow
                };
            }
            finally
            {
                _checkoutSemaphore.Release();
                _logger.LogInformation("Released checkout semaphore for user: {UserName}", userName);
            }
        }
    }

    public class CheckoutResult
    {
        public bool Success { get; set; }
        public required string Message { get; set; }
        public List<string> Items { get; set; } = [];
        public DateTime? CheckoutTime { get; set; }
    }
}
