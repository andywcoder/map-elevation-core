using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.Lib.Services;
using Santolibre.Map.Elevation.WebService.ApiControllers.v1.Models;
using System.Linq;

namespace Santolibre.Map.Elevation.WebService.ApiControllers.v1
{
    [Route("api/v1")]
    public class ElevationController : ControllerBase
    {
        private readonly IElevationService _elevationService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ElevationController(IElevationService elevationService, IMapper mapper, IConfiguration configuration)
        {
            _elevationService = elevationService;
            _mapper = mapper;
            _configuration = configuration;
        }

        [Route("elevation")]
        [HttpGet]
        public IActionResult Elevation([FromQuery]ElevationQuery elevationQuery)
        {
            var points = GooglePolyline.Decode(elevationQuery.EncodedPoints).ToList();
            if (!points.Any())
            {
                return BadRequest(new { Error = "Couldn't decode points" });
            }

            var elevationModelType = _elevationService.GetElevations(points, elevationQuery.SmoothingMode, _configuration.GetValue<int>("AppSettings:ElevationQueryMaxNodes"));
            if (elevationModelType.HasValue)
            {
                return Ok(_mapper.Map<ElevationResponse>(points));
            }
            else
            {
                return NotFound(new { Error = "No elevation data for this area" });
            }
        }
    }
}
