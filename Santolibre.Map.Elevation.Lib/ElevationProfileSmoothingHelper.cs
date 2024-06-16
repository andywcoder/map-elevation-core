using System.Collections.Generic;
using System.Linq;

namespace Santolibre.Map.Elevation.Lib
{
    public static class ElevationProfileSmoothingHelper
    {
        public static void WindowSmooth(List<IGeoLocation> points, float[] smoothingFilter)
        {
            for (var i = smoothingFilter.Length / 2; i < points.Count - smoothingFilter.Length / 2; i++)
            {
                float elevationSum = 0;
                for (var j = -smoothingFilter.Length / 2; j <= smoothingFilter.Length / 2; j++)
                {
                    elevationSum += smoothingFilter[j + smoothingFilter.Length / 2] * points[i - j].Elevation!.Value;
                }
                points[i].Elevation = elevationSum / smoothingFilter.Sum();
            }
        }

        public static void FeedbackSmooth(List<IGeoLocation> points, float feedbackWeight, float currentWeight)
        {
            var filteredValue = points[0].Elevation!.Value;
            for (var i = 0; i < points.Count; i++)
            {
                filteredValue = (filteredValue * feedbackWeight + points[i].Elevation!.Value * currentWeight) / (feedbackWeight + currentWeight);
                points[i].Elevation = filteredValue;
            }
            filteredValue = points[points.Count - 1].Elevation!.Value;
            for (var i = points.Count - 1; i >= 0; i--)
            {
                filteredValue = (filteredValue * feedbackWeight + points[i].Elevation!.Value * currentWeight) / (feedbackWeight + currentWeight);
                points[i].Elevation = filteredValue;
            }
        }
    }
}
