using Santolibre.Map.Elevation.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public interface IElevationService
    {
        DigitalElevationModelType? LookupElevations(List<IGeoLocation> points, SmoothingMode smoothingMode, int maxPoints);
    }
}
