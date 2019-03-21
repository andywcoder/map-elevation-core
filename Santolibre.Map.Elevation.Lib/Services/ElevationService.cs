using Microsoft.Extensions.Configuration;
using Santolibre.Map.Elevation.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Santolibre.Map.Elevation.Lib.Services
{
    public class ElevationService : IElevationService
    {
        private readonly ICacheService _cacheService;
        private readonly IConfiguration _configuration;

        public ElevationService(ICacheService cacheService, IConfiguration configuration)
        {
            _cacheService = cacheService;
            _configuration = configuration;
        }

        private void WindowSmooth(List<IGeoLocation> points, float[] smoothingFilter)
        {
            for (var i = smoothingFilter.Length / 2; i < points.Count - smoothingFilter.Length / 2; i++)
            {
                float elevationSum = 0;
                for (var j = -smoothingFilter.Length / 2; j <= smoothingFilter.Length / 2; j++)
                {
                    elevationSum += smoothingFilter[j + smoothingFilter.Length / 2] * points[i - j].Elevation;
                }
                points[i].Elevation = elevationSum / smoothingFilter.Sum();
            }
        }

        private void FeedbackSmooth(List<IGeoLocation> points, float feedbackWeight, float currentWeight)
        {
            var filteredValue = points[0].Elevation;
            for (var i = 0; i < points.Count; i++)
            {
                filteredValue = (filteredValue * feedbackWeight + points[i].Elevation * currentWeight) / (feedbackWeight + currentWeight);
                points[i].Elevation = filteredValue;
            }
            filteredValue = points[points.Count - 1].Elevation;
            for (var i = points.Count - 1; i >= 0; i--)
            {
                filteredValue = (filteredValue * feedbackWeight + points[i].Elevation * currentWeight) / (feedbackWeight + currentWeight);
                points[i].Elevation = filteredValue;
            }
        }

        private DigitalElevationModelType? LookupElevations(List<IGeoLocation> points)
        {
            var dataPath = _configuration.GetValue<string>("AppSettings:DemFolder");

            Dictionary<string, object> cache;
            if (_cacheService.GetValue("SRTM") != null)
            {
                cache = (Dictionary<string, object>)_cacheService.GetValue("SRTM");
                if (cache.Count > 10)
                {
                    var removeKeys = cache.Keys.ToList().Take(cache.Count - 10).ToList();
                    removeKeys.ForEach(x => cache.Remove(x));
                }
            }
            else
            {
                cache = new Dictionary<string, object>();
                _cacheService.Add("SRTM", cache, DateTimeOffset.UtcNow + new TimeSpan(1, 0, 0));
            }
            var areFilesAvailable = true;

            // Check HGT files
            foreach (var point in points)
            {
                var filename = HGT.GetFilename(point.Latitude, point.Longitude);
                if (File.Exists(Path.Combine(dataPath, filename)))
                {
                    if (!cache.ContainsKey(filename))
                    {
                        using (Stream stream = new FileStream(Path.Combine(dataPath, filename), FileMode.Open))
                        {
                            var hgt = HGT.Create(stream);
                            cache.Add(filename, hgt);
                        }
                    }
                }
                else
                {
                    areFilesAvailable = false;
                }
            }
            if (areFilesAvailable)
            {
                foreach (var point in points)
                {
                    var hgt = (HGT)cache[HGT.GetFilename(point.Latitude, point.Longitude)];
                    point.Elevation = hgt.GetElevation(point.Latitude, point.Longitude);
                }
                return DigitalElevationModelType.SRTM1;
            }

            // Check geotiff files
            areFilesAvailable = true;
            foreach (var point in points)
            {
                var filename = GeoTiff.GetFilename(point.Latitude, point.Longitude);
                if (File.Exists(Path.Combine(dataPath, filename)))
                {
                    if (!cache.ContainsKey(filename))
                    {
                        using (Stream stream = new FileStream(Path.Combine(dataPath, filename), FileMode.Open))
                        {
                            var hgt = GeoTiff.Create(stream);
                            cache.Add(filename, hgt);
                        }
                    }
                }
                else
                {
                    areFilesAvailable = false;
                }
            }
            if (areFilesAvailable)
            {
                foreach (var point in points)
                {
                    var geoTiff = (GeoTiff)cache[GeoTiff.GetFilename(point.Latitude, point.Longitude)];
                    point.Elevation = geoTiff.GetElevation(point.Latitude, point.Longitude);
                }
                return DigitalElevationModelType.SRTM3;
            }

            return null;
        }

        public DigitalElevationModelType? LookupElevations(List<IGeoLocation> points, SmoothingMode smoothingMode, int maxPoints)
        {
            points = points.Take(maxPoints).ToList();

            var demType = LookupElevations(points);
            if (demType.HasValue)
            {
                if (demType.Value == DigitalElevationModelType.SRTM1)
                {
                    switch (smoothingMode)
                    {
                        case SmoothingMode.WindowSmooth:
                            WindowSmooth(points, new float[] { 0.1f, 1f, 0.1f });
                            break;
                        case SmoothingMode.FeedbackSmooth:
                            FeedbackSmooth(points, 1, 3);
                            break;
                    }
                }
                else
                {
                    switch (smoothingMode)
                    {
                        case SmoothingMode.WindowSmooth:
                            WindowSmooth(points, new float[] { 1, 2, 3, 2, 1 });
                            break;
                        case SmoothingMode.FeedbackSmooth:
                            FeedbackSmooth(points, 3, 1);
                            break;
                    }
                }

                return demType.Value;
            }
            else
            {
                return null;
            }
        }
    }
}
