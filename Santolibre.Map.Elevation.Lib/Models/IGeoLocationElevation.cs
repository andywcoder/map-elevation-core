namespace Santolibre.Map.Elevation.Lib.Models
{
    public interface IGeoLocationElevation
    {
        float Longitude { get; set; }
        float Latitude { get; set; }
        float Elevation { get; set; }
    }
}
