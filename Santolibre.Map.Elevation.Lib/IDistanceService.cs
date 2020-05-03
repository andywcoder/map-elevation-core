using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib
{
    public interface IDistanceService
    {
        void CalculateDistances(List<IGeoPoint> points);
    }
}
