using Santolibre.Map.Elevation.Lib.Models;
using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public interface IElevationService
    {
        DigitalElevationModelType? GetElevations(List<INode> nodes, SmoothingMode smoothingMode, int maxNodes);
    }
}
