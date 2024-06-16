using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib
{
    public interface IElevationService
    {
        DigitalElevationModelType LookupElevations(List<IGeoLocation> points, SmoothingMode smoothingMode, int maxPoints);
    }
}
