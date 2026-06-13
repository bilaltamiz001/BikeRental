using BikeRental.Data.Models;
using BikeRental.Models.Api;
using BikeRental.Services;
using Microsoft.AspNetCore.Mvc;

namespace BikeRental.Controllers
{
    /// <summary>
    /// API controller for bike operations (beach cruisers and mountain bikes).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BikesController : ControllerBase
    {
        private readonly IBeachCruiserService _beachService;
        private readonly IMountainBikeService _mountainService;
        private readonly ILogger<BikesController> _logger;

        public BikesController(
            IBeachCruiserService beachService,
            IMountainBikeService mountainService,
            ILogger<BikesController> logger)
        {
            _beachService = beachService ?? throw new ArgumentNullException(nameof(beachService));
            _mountainService = mountainService ?? throw new ArgumentNullException(nameof(mountainService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets all beach cruisers.
        /// </summary>
        /// <response code="200">Returns the list of beach cruisers.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("beach")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetBeachCruisers()
        {
            try
            {
                _logger.LogInformation("GetBeachCruisers endpoint called");
                var bikes = _beachService.GetAll();
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
        /// Gets a specific beach cruiser by ID.
        /// </summary>
        /// <param name="id">The beach cruiser ID.</param>
        /// <response code="200">Returns the requested beach cruiser.</response>
        /// <response code="404">Beach cruiser not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("beach/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetBeachCruiser(int id)
        {
            var bike = _beachService.GetById(id);
            if (bike == null)
                return NotFound(ApiResponse<object>.FailureResponse($"Beach cruiser #{id} not found."));

            return Ok(ApiResponse<BeachCruiser>.SuccessResponse(bike, "Beach cruiser retrieved successfully"));
        }

        /// <summary>
        /// Gets all mountain bikes.
        /// </summary>
        /// <response code="200">Returns the list of mountain bikes.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("mountain")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetMountainBikes()
        {
            try
            {
                _logger.LogInformation("GetMountainBikes endpoint called");
                var bikes = _mountainService.GetAll();
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
        /// Gets a specific mountain bike by ID.
        /// </summary>
        /// <param name="id">The mountain bike ID.</param>
        /// <response code="200">Returns the requested mountain bike.</response>
        /// <response code="404">Mountain bike not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("mountain/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetMountainBike(int id)
        {
            var bike = _mountainService.GetById(id);
            if (bike == null)
                return NotFound(ApiResponse<object>.FailureResponse($"Mountain bike #{id} not found."));

            return Ok(ApiResponse<MountainBike>.SuccessResponse(bike, "Mountain bike retrieved successfully"));
        }

        /// <summary>
        /// Rents a beach cruiser.
        /// </summary>
        /// <param name="id">The beach cruiser ID to rent.</param>
        /// <response code="200">Beach cruiser rented successfully.</response>
        /// <response code="404">Beach cruiser not found.</response>
        /// <response code="409">Beach cruiser is not available for rental.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("beach/{id}/rent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult RentBeachCruiser(int id)
        {
            _beachService.RentBike(id);
            return Ok(ApiResponse.SuccessResponse($"Beach cruiser #{id} rented successfully"));
        }

        /// <summary>
        /// Rents a mountain bike.
        /// </summary>
        /// <param name="id">The mountain bike ID to rent.</param>
        /// <response code="200">Mountain bike rented successfully.</response>
        /// <response code="404">Mountain bike not found.</response>
        /// <response code="409">Mountain bike is not available for rental.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("mountain/{id}/rent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult RentMountainBike(int id)
        {
            _mountainService.RentBike(id);
            return Ok(ApiResponse.SuccessResponse($"Mountain bike #{id} rented successfully"));
        }

        /// <summary>
        /// Resets beach cruisers to default inventory state.
        /// </summary>
        /// <response code="200">Beach cruisers reset successfully.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("beach/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ResetBeachCruisers()
        {
            _beachService.ResetToDefaults();
            return Ok(ApiResponse.SuccessResponse("Beach cruisers reset to default inventory."));
        }

        /// <summary>
        /// Resets mountain bikes to default inventory state.
        /// </summary>
        /// <response code="200">Mountain bikes reset successfully.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost("mountain/reset")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ResetMountainBikes()
        {
            _mountainService.ResetToDefaults();
            return Ok(ApiResponse.SuccessResponse("Mountain bikes reset to default inventory."));
        }
    }
}
