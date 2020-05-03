namespace Santolibre.Map.Elevation.Lib
{
    public interface IGeoLocation
    {
        float Longitude { get; set; }
        float Latitude { get; set; }
        float? Elevation { get; set; }
    }
}
