namespace Santolibre.Map.Elevation.Lib.Models
{
    public interface INode : IGeoLocationElevation
    {
        float Distance { get; set; }

        INode GetDestinationNode(float bearing, double distance);
        float GetDistanceToNode(INode node);
    }
}
