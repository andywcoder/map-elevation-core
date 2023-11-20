using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Santolibre.Map.Elevation.Lib
{
    public class ElevationService : IElevationService
    {
        private readonly IDemFileCache _cacheService;
        private readonly IConfiguration _configuration;

        public ElevationService(IDemFileCache cacheService, IConfiguration configuration)
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
                    elevationSum += smoothingFilter[j + smoothingFilter.Length / 2] * points[i - j].Elevation.Value;
                }
                points[i].Elevation = elevationSum / smoothingFilter.Sum();
            }
        }

        private void FeedbackSmooth(List<IGeoLocation> points, float feedbackWeight, float currentWeight)
        {
            var filteredValue = points[0].Elevation.Value;
            for (var i = 0; i < points.Count; i++)
            {
                filteredValue = (filteredValue * feedbackWeight + points[i].Elevation.Value * currentWeight) / (feedbackWeight + currentWeight);
                points[i].Elevation = filteredValue;
            }
            filteredValue = points[points.Count - 1].Elevation.Value;
            for (var i = points.Count - 1; i >= 0; i--)
            {
                filteredValue = (filteredValue * feedbackWeight + points[i].Elevation.Value * currentWeight) / (feedbackWeight + currentWeight);
                points[i].Elevation = filteredValue;
            }
        }

        private DigitalElevationModelType? LookupElevations(List<IGeoLocation> points)
        {
            var dataPath = _configuration["AppSettings:DemFolder"];
            var demFileTypes = _configuration["AppSettings:DemFileTypes"].Split(',');
            var cache = InitializeCache();

            bool areFilesAvailable;

            if (demFileTypes.Contains(typeof(HgtRaw).Name))
            {
                areFilesAvailable = CheckDemFilesAvailability(points, dataPath, typeof(HgtRaw), cache);
                if (areFilesAvailable)
                {
                    foreach (var point in points)
                    {
                        var hgt = (IDemFile)cache[HgtRaw.GetFilename(point.Latitude, point.Longitude)];
                        point.Elevation = hgt.GetElevation(point.Latitude, point.Longitude);
                    }
                    return DigitalElevationModelType.SRTM1;
                }
            }

            if (demFileTypes.Contains(typeof(GeoTiff).Name))
            {
                areFilesAvailable = CheckDemFilesAvailability(points, dataPath, typeof(GeoTiff), cache);
                if (areFilesAvailable)
                {
                    foreach (var point in points)
                    {
                        var geoTiff = (IDemFile)cache[GeoTiff.GetFilename(point.Latitude, point.Longitude)];
                        point.Elevation = geoTiff.GetElevation(point.Latitude, point.Longitude);
                    }
                    return DigitalElevationModelType.SRTM3;
                }
            }

            if (demFileTypes.Contains(typeof(HgtPng).Name))
            {
                areFilesAvailable = CheckDemFilesAvailability(points, dataPath, typeof(HgtPng), cache);
                if (areFilesAvailable)
                {
                    foreach (var point in points)
                    {
                        var hgtImage = (IDemFile)cache[HgtPng.GetFilename(point.Latitude, point.Longitude)];
                        point.Elevation = hgtImage.GetElevation(point.Latitude, point.Longitude);
                    }
                    return DigitalElevationModelType.SRTM1;
                }
            }

            return null;
        }

        private Dictionary<string, object> InitializeCache()
        {
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

            return cache;
        }

        private static bool CheckDemFilesAvailability(List<IGeoLocation> points, string dataPath, Type demFileType, Dictionary<string, object> cache)
        {
            var areFilesAvailable = true;
            foreach (var point in points)
            {
                string filename;
                if (demFileType == typeof(HgtRaw))
                {
                    filename = HgtRaw.GetFilename(point.Latitude, point.Longitude);
                }
                else if (demFileType == typeof(GeoTiff))
                {
                    filename = GeoTiff.GetFilename(point.Latitude, point.Longitude);
                }
                else if (demFileType == typeof(HgtPng))
                {
                    filename = HgtPng.GetFilename(point.Latitude, point.Longitude);
                }
                else
                {
                    throw new Exception("DEM file type not supported");
                }
                if (File.Exists(Path.Combine(dataPath, filename)))
                {
                    if (!cache.ContainsKey(filename))
                    {
                        using (var stream = new FileStream(Path.Combine(dataPath, filename), FileMode.Open, FileAccess.Read))
                        {
                            IDemFile demFile;
                            if (demFileType == typeof(HgtRaw))
                            {
                                demFile = HgtRaw.Create(stream);
                            }
                            else if (demFileType == typeof(GeoTiff))
                            {
                                demFile = GeoTiff.Create(stream);
                            }
                            else if (demFileType == typeof(HgtPng))
                            {
                                demFile = HgtPng.Create(stream);
                            }
                            else
                            {
                                throw new Exception("DEM file type not supported");
                            }
                            cache.Add(filename, demFile);
                        }
                    }
                }
                else
                {
                    areFilesAvailable = false;
                }
            }

            return areFilesAvailable;
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
