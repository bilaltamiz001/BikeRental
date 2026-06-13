using BikeRental.Attributes;
using BikeRental.Controllers.ApiVersioning;
using BikeRental.Data.Models;
using BikeRental.Models.Api;
using BikeRental.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental.Controllers.V1_1
{
    /// <summary>
    /// API controller for bike operations (v1.1) with async/await support and caching.
    /// </summary>
    [ApiController]
    [Route("api/v1.1/[controller]")]
    [Produces("application/json")]
    public class BikesAsyncController : ControllerBase
    {
        private readonly IBeachCruiserAsyncService _beachService;
        private readonly IMountainBikeAsyncService _mountainService;
        private readonly ILogger<BikesAsyncController> _logger;

        public BikesAsyncController(
            IBeachCruiserAsyncService beachService,
            IMountainBikeAsyncService mountainService,
            ILogger<BikesAsyncController> logger)
        {
            _beachService = beachService ?? throw new ArgumentNullException(nameof(beachService));
            _mountainService = mountainService ?? throw new ArgumentNullException(nameof(mountainService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all beach cruisers asynchronously with response caching.
        /// </summary>
        /// <response code="200">Returns the list of beach cruisers.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("beach")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [CacheableResponse(durationInSeconds: 300)]
        public async Task<IActionResult> GetBeachCruisersAsync()
        {
            try
            {
                _logger.LogInformation("GetBeachCruisersAsync endpoint called");
                var bikes = await _beachService.GetAllAsync();
                _logger.LogInformation("Successfully retrieved {BikeCount} beach cruisers", bikes.Count);
                return Ok(ApiResponse<List<BeachCruiser>>.SuccessResponse(bikes, "Beach cruisers retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving beach cruisers");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific beach cruiser by ID asynchronously.
        /// </summary>
        /// <param name="id">The beach cruiser ID.</param>
        /// <response code="200">Returns the requested beach cruiser.</response>
        /// <response code="404">Beach cruiser not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("beach/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBeachCruiserAsync(int id)
        {
            var bike = await _beachService.GetByIdAsync(id);
            if (bike == null)
                return NotFound(ApiResponse<object>.FailureResponse($"Beach cruiser #{id} not found."));

            return Ok(ApiResponse<BeachCruiser>.SuccessResponse(bike, "Beach cruiser retrieved successfully"));
        }

        /// <summary>
        /// Gets all mountain bikes asynchronously with response caching.
        /// </summary>
        /// <response code="200">Returns the list of mountain bikes.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("mountain")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [CacheableResponse(durationInSeconds: 300)]
        public async Task<IActionResult> GetMountainBikesAsync()
        {
            try
            {
                _logger.LogInformation("GetMountainBikesAsync endpoint called");
                var bikes = await _mountainService.GetAllAsync();
                _logger.LogInformation("Successfully retrieved {BikeCount} mountain bikes", bikes.Count);
                return Ok(ApiResponse<List<MountainBike>>.SuccessResponse(bikes, "Mountain bikes retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving mountain bikes");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific mountain bike by ID asynchronously.
        /// </summary>
        /// <param name="id">The mountain bike ID.</param>
        /// <response code="200">Returns the requested mountain bike.</response>
        /// <response code="404">Mountain bike not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("mountain/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMountainBikeAsync(int id)
        {
            var bike = await _mountainService.GetByIdAsync(id);
            if (bike == null)
                return NotFound(ApiResponse<object>.FailureResponse($"Mountain bike #{id} not found."));

            return Ok(ApiResponse<MountainBike>.SuccessResponse(bike, "Mountain bike retrieved successfully"));
        }

        /// <summary>
        /// Rents a beach cruiser asynchronously.
        /// </summary>
        /// <param name="bikeId">The bike ID to rent.</param>
        /// <response code="200">Bike rented successfully.</response>
        /// <response code="404">Bike not found.</response>
        /// <response code="409">Bike not available.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("beach/{bikeId}/rent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RentBeachCruiserAsync(int bikeId)
        {
            try
            {
                _logger.LogInformation("RentBeachCruiserAsync called for bike {BikeId}", bikeId);
                var result = await _beachService.RentBikeAsync(bikeId);
                return Ok(ApiResponse<bool>.SuccessResponse(result, $"Beach cruiser {bikeId} rented successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renting beach cruiser {BikeId}", bikeId);
                throw;
            }
        }

        /// <summary>
        /// Rents a mountain bike asynchronously.
        /// </summary>
        /// <param name="bikeId">The bike ID to rent.</param>
        /// <response code="200">Bike rented successfully.</response>
        /// <response code="404">Bike not found.</response>
        /// <response code="409">Bike not available.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("mountain/{bikeId}/rent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RentMountainBikeAsync(int bikeId)
        {
            try
            {
                _logger.LogInformation("RentMountainBikeAsync called for bike {BikeId}", bikeId);
                var result = await _mountainService.RentBikeAsync(bikeId);
                return Ok(ApiResponse<bool>.SuccessResponse(result, $"Mountain bike {bikeId} rented successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renting mountain bike {BikeId}", bikeId);
                throw;
            }
        }

        /// <summary>
        /// Resets beach cruisers to default availability.
        /// </summary>
        /// <response code="200">Reset successful.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("beach/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetBeachCruisersAsync()
        {
            try
            {
                _logger.LogInformation("ResetBeachCruisersAsync called");
                await _beachService.ResetToDefaultsAsync();
                return Ok(ApiResponse.SuccessResponse("Beach cruisers reset successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting beach cruisers");
                throw;
            }
        }

        /// <summary>
        /// Resets mountain bikes to default availability.
        /// </summary>
        /// <response code="200">Reset successful.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("mountain/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetMountainBikesAsync()
        {
            try
            {
                _logger.LogInformation("ResetMountainBikesAsync called");
                await _mountainService.ResetToDefaultsAsync();
                return Ok(ApiResponse.SuccessResponse("Mountain bikes reset successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting mountain bikes");
                throw;
            }
        }
    }
}
