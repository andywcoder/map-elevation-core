using Santolibre.Map.Elevation.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public interface IDistanceService
    {
        void CalculateDistances(List<IGeoPoint> points);
    }
}
