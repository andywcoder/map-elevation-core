using System;

namespace Santolibre.Map.Elevation.Lib.Models
{
    public class Node : INode
    {
        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Elevation { get; set; }
        public float Distance { get; set; }

        public INode GetDestinationNode(float bearing, double distance)
        {
            distance = distance / 6371;
            bearing = bearing.ToRad();
            var lat1 = Latitude.ToRad();
            var lon1 = Longitude.ToRad();
            var lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(distance) + Math.Cos(lat1) * Math.Sin(distance) * Math.Cos(bearing));
            var lon2 = lon1 + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance) * Math.Cos(lat1), Math.Cos(distance) - Math.Sin(lat1) * Math.Sin(lat2));
            lon2 = (lon2 + 3 * Math.PI) % (2 * Math.PI) - Math.PI;
            return new Node() { Latitude = ((float)lat2).ToDeg(), Longitude = ((float)lon2).ToDeg() };
        }

        public float GetDistanceToNode(INode node)
        {
            var R = 6371;
            var dLat = (node.Latitude - Latitude).ToRad();
            var dLon = (node.Longitude - Longitude).ToRad();
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Latitude.ToRad()) * Math.Cos(node.Latitude.ToRad()) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c;
            return (float)d;
        }
    }
}
