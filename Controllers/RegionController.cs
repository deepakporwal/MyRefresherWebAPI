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

        public RegionController(IMapper mapper)
        {
            _mapper = mapper;
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
            var regions = new List<Region>
            {
                new Region { Id = 1, Name = "Region 1", Code = "R1" },
                new Region { Id = 2, Name = "Region 2", Code = "R2" },
                new Region { Id = 3, Name = "Region 3", Code = "R3" }
            };

            var regionDtos = _mapper.Map<List<RegionDTO>>(regions);

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
            return Ok(new { message = "Region added successfully." });
        }

    }
}
