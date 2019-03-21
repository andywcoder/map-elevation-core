using Santolibre.Map.Elevation.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public class DistanceService : IDistanceService
    {
        public void CalculateDistances(List<IGeoPoint> points)
        {
            var totalDistance = 0f;
            for (var i = 1; i < points.Count; i++)
            {
                var distance = points[i - 1].GetDistanceToPoint(points[i]);
                totalDistance += distance;
                points[i].Distance = totalDistance;
            }
        }
    }
}
