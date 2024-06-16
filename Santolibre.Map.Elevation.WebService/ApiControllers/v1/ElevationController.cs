using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Santolibre.Map.Elevation.Lib;
using Santolibre.Map.Elevation.WebService.ApiControllers.v1.Models;
using System.Linq;

namespace Santolibre.Map.Elevation.WebService.ApiControllers.v1
{
    [Route("api/v1")]
    public class ElevationController : ControllerBase
    {
        private readonly IElevationService _elevationService;
        private readonly IDistanceService _distanceService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ElevationController(IElevationService elevationService, IDistanceService distanceService, IMapper mapper, IConfiguration configuration)
        {
            _elevationService = elevationService;
            _distanceService = distanceService;
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

            var elevationModelType = _elevationService.LookupElevations(points.ConvertAll(x => (IGeoLocation)x), elevationQuery.SmoothingMode, _configuration.GetValue<int>("AppSettings:ElevationQueryMaxPoints"));

            _distanceService.CalculateDistances(points);

            if (elevationModelType != DigitalElevationModelType.None)
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
