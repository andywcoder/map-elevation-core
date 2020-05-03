namespace Santolibre.Map.Elevation.Lib
{
    public interface IGeoPoint : IGeoLocation
    {
        float Distance { get; set; }

        IGeoPoint GetDestinationPoint(float bearing, double distance);
        float GetDistanceToPoint(IGeoPoint point);
    }
}
