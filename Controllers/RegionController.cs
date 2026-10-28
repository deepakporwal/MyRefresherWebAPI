using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyRefresherWebAPI.Models.Domain;
using MyRefresherWebAPI.Models.DTO;

namespace MyRefresherWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RegionController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<RegionController> _logger;

        public RegionController(IMapper mapper, ILogger<RegionController> logger)
        {
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet("regions")]
        [Authorize(Policy = "Regions.Read")]
        [ProducesResponseType(typeof(IEnumerable<RegionDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetRegions()
        {
            _logger.LogInformation(
                "Retrieving regions for user {User}.",
                User.Identity?.Name ?? "unknown");

            var regions = new List<Region>
            {
                new Region { Id = 1, Name = "Region 1", Code = "R1" },
                new Region { Id = 2, Name = "Region 2", Code = "R2" },
                new Region { Id = 3, Name = "Region 3", Code = "R3" }
            };

            var regionDtos = _mapper.Map<List<RegionDTO>>(regions);

            _logger.LogInformation(
                "Retrieved {RegionCount} regions for user {User}.",
                regionDtos.Count,
                User.Identity?.Name ?? "unknown");

            return Ok(regionDtos);
        }

        [HttpPost("regions")]
        [Authorize(Policy = "Regions.Write")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult AddRegion([FromBody] RegionDTO regionDTO)
        {
            _logger.LogInformation(
                "Adding region {RegionId} for user {User}.",
                regionDTO.Id,
                User.Identity?.Name ?? "unknown");

            return Ok(new { message = "Region added successfully." });
        }

    }
}
