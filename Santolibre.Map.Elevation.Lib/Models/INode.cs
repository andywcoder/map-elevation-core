namespace Santolibre.Map.Elevation.Lib.Models
{
    public interface INode
    {
        float Longitude { get; set; }
        float Latitude { get; set; }
        float Elevation { get; set; }
        float Distance { get; set; }

        INode GetDestinationNode(float bearing, double distance);
        float GetDistanceToNode(INode node);
    }
}
