using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BikeRental.Data;
using BikeRental.Data.Models;
using BikeRental.Exceptions;
using Microsoft.Extensions.Logging;

namespace BikeRental.Services
{
    /// <summary>
    /// Async service for accessory operations and order processing.
    /// Provides async/await support for improved scalability.
    /// </summary>
    public class AccessoryAsyncService : IAccessoryAsyncService
    {
        private readonly IAccessoryAsyncRepository _repo;
        private readonly ILogger<AccessoryAsyncService> _logger;
        private List<Accessory>? _defaults;
        private static readonly HashSet<int> _bundleIds = new HashSet<int> { 1, 3 };
        private const double BundleDiscountRate = 0.10;

        public AccessoryAsyncService(IAccessoryAsyncRepository repo, ILogger<AccessoryAsyncService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all accessories asynchronously.
        /// </summary>
        public async Task<List<Accessory>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all accessories");
                return await _repo.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accessories");
                throw new DataAccessException("Failed to fetch accessories", ex);
            }
        }

        /// <summary>
        /// Gets accessories compatible with a specific bike type asynchronously.
        /// </summary>
        public async Task<List<Accessory>> GetCompatibleWithAsync(string bikeType)
        {
            if (string.IsNullOrWhiteSpace(bikeType))
            {
                _logger.LogWarning("GetCompatibleWithAsync called with invalid bike type");
                throw new ValidationException("Bike type cannot be null or empty");
            }

            var lower = bikeType.ToLower();
            _logger.LogInformation("Fetching accessories compatible with {BikeType}", lower);

            var accessories = await GetAllAsync();
            return accessories
                .Where(a => Array.IndexOf(a.CompatibleWith, lower) >= 0
                         || Array.IndexOf(a.CompatibleWith, "all") >= 0)
                .ToList();
        }

        /// <summary>
        /// Processes an accessory order asynchronously with validation and bundle discounts.
        /// </summary>
        public async Task<AccessoryRequestResult> ProcessOrderAsync(Dictionary<int, int> quantities)
        {
            try
            {
                if (quantities == null || !quantities.Any(q => q.Value > 0))
                {
                    _logger.LogWarning("Order processing failed: no items selected");
                    return Fail("No items selected.");
                }

                var inventory = await GetAllAsync();

                // Validation pass: verify inventory availability
                foreach (var kvp in quantities.Where(q => q.Value > 0))
                {
                    var item = inventory.FirstOrDefault(a => a.AccessoryID == kvp.Key);
                    if (item == null)
                    {
                        _logger.LogWarning("Order processing failed: accessory {AccessoryId} not found", kvp.Key);
                        return Fail($"Accessory #{kvp.Key} not found.");
                    }

                    if (item.StockCount < kvp.Value)
                    {
                        _logger.LogWarning("Order processing failed: insufficient stock for accessory {AccessoryId}", kvp.Key);
                        return Fail($"Insufficient stock for accessory #{kvp.Key}. Available: {item.StockCount}, Requested: {kvp.Value}");
                    }
                }

                // Process order
                double totalPrice = 0;
                var bundleItemsInOrder = quantities.Where(q => q.Value > 0 && _bundleIds.Contains(q.Key)).Count();

                foreach (var kvp in quantities.Where(q => q.Value > 0))
                {
                    var item = inventory.First(a => a.AccessoryID == kvp.Key);
                    totalPrice += item.UnitPrice * kvp.Value;
                    item.StockCount -= kvp.Value;
                }

                double discountAmount = bundleItemsInOrder >= 2 ? totalPrice * BundleDiscountRate : 0;
                bool bundleDiscountApplied = discountAmount > 0;

                // Save updated inventory
                await _repo.SaveAsync(inventory);

                _logger.LogInformation("Order processed successfully. Total: ${Total}, Discount: ${Discount}, Final: ${Final}",
                    totalPrice, discountAmount, totalPrice - discountAmount);

                return new AccessoryRequestResult
                {
                    Success = true,
                    Message = "Order processed successfully",
                    TotalPrice = totalPrice,
                    DiscountAmount = discountAmount,
                    BundleDiscountApplied = bundleDiscountApplied
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing accessory order");
                throw;
            }
        }

        /// <summary>
        /// Restores all accessories to their original stock levels asynchronously.
        /// </summary>
        public async Task ResetToDefaultsAsync()
        {
            try
            {
                _logger.LogInformation("Resetting accessories to defaults");
                _defaults ??= await GetAllAsync();
                await _repo.SaveAsync(new List<Accessory>(_defaults));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting accessories to defaults");
                throw;
            }
        }

        private static AccessoryRequestResult Fail(string message) =>
            new AccessoryRequestResult { Success = false, Message = message };
    }
}
