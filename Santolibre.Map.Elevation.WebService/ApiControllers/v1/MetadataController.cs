using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Santolibre.Map.Elevation.WebService.ApiControllers.v1.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Elevation.WebService.ApiControllers.v1
{
    [Route("api/v1")]
    public class MetadataController : ControllerBase
    {
        private readonly Lib.IMetadataService _metadataService;
        private readonly IMapper _mapper;

        public MetadataController(Lib.IMetadataService metadataService, IMapper mapper)
        {
            _metadataService = metadataService;
            _mapper = mapper;
        }

        [Route("metadata")]
        [HttpGet]
        public List<SrtmRectangle> Metadata()
        {
            var srtmRectangles = new List<SrtmRectangle>();
            srtmRectangles.AddRange(_mapper.Map<List<SrtmRectangle>>(_metadataService.GetSRTM1Rectangles()));
            srtmRectangles.AddRange(_mapper.Map<List<SrtmRectangle>>(_metadataService.GetSRTM3Rectangles()));
            return srtmRectangles;
        }
    }
}
