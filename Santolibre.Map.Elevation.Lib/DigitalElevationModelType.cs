using System.Text.Json.Serialization;

namespace Santolibre.Map.Elevation.Lib
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DigitalElevationModelType
    {
        None,
        SRTM1,
        SRTM3
    }
}
