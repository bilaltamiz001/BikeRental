using System;
using System.Collections.Generic;
using System.Linq;
using BikeRental.Data;
using BikeRental.Data.Models;
using BikeRental.Exceptions;
using Microsoft.Extensions.Logging;

namespace BikeRental.Services
{
    /// <summary>
    /// Service for accessory operations and order processing.
    /// Handles inventory management, compatibility checks, and bundle discounts.
    /// </summary>
    public class AccessoryService : IAccessoryService
    {
        private readonly IAccessoryRepository _repo;
        private readonly ILogger<AccessoryService> _logger;
        private readonly List<Accessory> _defaults;
        private static readonly HashSet<int> _bundleIds = new HashSet<int> { 1, 3 };
        private const double BundleDiscountRate = 0.10;

        public AccessoryService(IAccessoryRepository repo, ILogger<AccessoryService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            try
            {
                _defaults = new List<Accessory>(_repo.GetAll());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load accessory defaults");
                throw new DataAccessException("Failed to initialize accessory service", ex);
            }
        }

        /// <summary>
        /// Restores all accessory stock counts to their original values.
        /// </summary>
        public void ResetToDefaults()
        {
            _logger.LogInformation("Resetting accessories to defaults");
            _repo.Save(_defaults);
        }

        /// <summary>
        /// Gets all accessories.
        /// </summary>
        public List<Accessory> GetAll()
        {
            try
            {
                _logger.LogInformation("Fetching all accessories");
                return _repo.GetAll();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accessories");
                throw new DataAccessException("Failed to fetch accessories", ex);
            }
        }

        /// <summary>
        /// Gets accessories compatible with a specific bike type.
        /// </summary>
        public List<Accessory> GetCompatibleWith(string bikeType)
        {
            if (string.IsNullOrWhiteSpace(bikeType))
            {
                _logger.LogWarning("GetCompatibleWith called with invalid bike type");
                throw new ValidationException("Bike type cannot be null or empty");
            }

            var lower = bikeType.ToLower();
            _logger.LogInformation("Fetching accessories compatible with {BikeType}", lower);

            return _repo.GetAll()
                .Where(a => Array.IndexOf(a.CompatibleWith, lower) >= 0
                         || Array.IndexOf(a.CompatibleWith, "all") >= 0)
                .ToList();
        }

        /// <summary>
        /// Processes an accessory order with validation, pricing, and bundle discounts.
        /// </summary>
        public AccessoryRequestResult ProcessOrder(Dictionary<int, int> quantities)
        {
            try
            {
                if (quantities == null || !quantities.Any(q => q.Value > 0))
                {
                    _logger.LogWarning("Order processing failed: no items selected");
                    return Fail("No items selected.");
                }

                var inventory = GetAll();

                // Validation pass: verify inventory availability
                foreach (var pair in quantities.Where(q => q.Value > 0))
                {
                    var acc = inventory.FirstOrDefault(a => a.AccessoryID == pair.Key);
                    if (acc == null)
                    {
                        _logger.LogWarning("Order processing failed: accessory {AccessoryId} not found", pair.Key);
                        return Fail($"Accessory #{pair.Key} does not exist.");
                    }

                    if (acc.StockCount < pair.Value)
                    {
                        _logger.LogWarning("Order processing failed: insufficient stock for accessory {AccessoryId}", pair.Key);
                        return Fail($"Only {acc.StockCount} {acc.Name}(s) in stock. Requesting {pair.Value} is not available.");
                    }
                }

                // Calculate subtotal
                double subtotal = 0;
                foreach (var pair in quantities.Where(q => q.Value > 0))
                {
                    var acc = inventory.First(a => a.AccessoryID == pair.Key);
                    subtotal += acc.UnitPrice * pair.Value;
                }

                // Apply bundle discount if applicable
                var requestedIds = new HashSet<int>(quantities.Where(q => q.Value > 0).Select(q => q.Key));
                bool bundleApplies = _bundleIds.IsSubsetOf(requestedIds);
                double discount = bundleApplies ? Math.Round(subtotal * BundleDiscountRate, 2) : 0;

                // Commit inventory changes
                foreach (var pair in quantities.Where(q => q.Value > 0))
                {
                    var acc = inventory.First(a => a.AccessoryID == pair.Key);
                    acc.StockCount -= pair.Value;
                }
                _repo.Save(inventory);

                _logger.LogInformation("Order processed successfully. Bundle applied: {BundleApplied}", bundleApplies);

                return new AccessoryRequestResult
                {
                    Success = true,
                    Message = bundleApplies
                        ? "Bundle discount applied! Water bottle + light: a lifestyle choice we support."
                        : "Order confirmed! Your accessories await.",
                    TotalPrice = Math.Round(subtotal - discount, 2),
                    DiscountAmount = discount,
                    BundleDiscountApplied = bundleApplies
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing accessory order");
                throw new DataAccessException("Failed to process order", ex);
            }
        }

        /// <summary>
        /// Creates a failure result for order processing.
        /// </summary>
        private static AccessoryRequestResult Fail(string message)
        {
            return new AccessoryRequestResult { Success = false, Message = message };
        }
    }
}
