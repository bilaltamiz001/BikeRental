using BikeRental.Attributes;
using BikeRental.Controllers.ApiVersioning;
using BikeRental.Data.Models;
using BikeRental.Models.Api;
using BikeRental.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental.Controllers.V1_1
{
    /// <summary>
    /// API controller for accessory operations (v1.1) with async/await support and caching.
    /// </summary>
    [ApiController]
    [Route("api/v1.1/[controller]")]
    [Produces("application/json")]
    public class AccessoriesAsyncController : ControllerBase
    {
        private readonly IAccessoryAsyncService _service;
        private readonly ILogger<AccessoriesAsyncController> _logger;

        public AccessoriesAsyncController(IAccessoryAsyncService service, ILogger<AccessoriesAsyncController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all accessories asynchronously with response caching.
        /// </summary>
        /// <response code="200">Returns the list of accessories.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [CacheableResponse(durationInSeconds: 300)]
        public async Task<IActionResult> GetAllAsync()
        {
            var accessories = await _service.GetAllAsync();
            return Ok(ApiResponse<List<Accessory>>.SuccessResponse(accessories, "Accessories retrieved successfully"));
        }

        /// <summary>
        /// Gets accessories compatible with a specific bike type asynchronously.
        /// </summary>
        /// <param name="bikeType">The bike type (mountain, beach, or all).</param>
        /// <response code="200">Returns the list of compatible accessories.</response>
        /// <response code="400">Invalid bike type.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("compatible/{bikeType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [CacheableResponse(durationInSeconds: 300, varyByQueryKeys: "bikeType")]
        public async Task<IActionResult> GetCompatibleAsync(string bikeType)
        {
            var accessories = await _service.GetCompatibleWithAsync(bikeType);
            return Ok(ApiResponse<List<Accessory>>.SuccessResponse(accessories, $"Compatible accessories for {bikeType} bikes retrieved"));
        }

        /// <summary>
        /// Processes an accessory order asynchronously with validation and bundle discounts.
        /// </summary>
        /// <param name="quantities">Dictionary mapping accessory ID to quantity.</param>
        /// <response code="200">Order processed successfully.</response>
        /// <response code="400">Invalid order (missing items, insufficient stock).</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("order")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessOrderAsync([FromBody] Dictionary<int, int> quantities)
        {
            if (quantities == null || quantities.Count == 0)
            {
                _logger.LogWarning("Order processing failed: no items specified");
                return BadRequest(ApiResponse.FailureResponse("No items specified."));
            }

            var result = await _service.ProcessOrderAsync(quantities);

            if (!result.Success)
            {
                _logger.LogWarning("Order processing failed: {Message}", result.Message);
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            }

            _logger.LogInformation("Order processed successfully. Total: ${Total}, Discount: ${Discount}",
                result.TotalPrice, result.DiscountAmount);

            return Ok(ApiResponse<object>.SuccessResponse(result, result.Message));
        }

        /// <summary>
        /// Resets accessories to default stock levels asynchronously.
        /// </summary>
        /// <response code="200">Accessories reset successfully.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetAsync()
        {
            try
            {
                await _service.ResetToDefaultsAsync();
                _logger.LogInformation("Accessories reset to default stock levels");
                return Ok(ApiResponse.SuccessResponse("Accessories reset successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting accessories");
                throw;
            }
        }
    }
}
