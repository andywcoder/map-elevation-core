using System.Collections.Generic;

namespace Santolibre.Map.Elevation.Lib
{
    public interface IMetadataService
    {
        List<SrtmRectangle> GetSRTM1Rectangles();
        List<SrtmRectangle> GetSRTM3Rectangles();
    }
}
