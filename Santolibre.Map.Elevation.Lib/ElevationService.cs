using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Santolibre.Map.Elevation.Lib
{
    public class ElevationService : IElevationService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ElevationService> _logger;
        private readonly string _dataPath;
        private readonly string[] _demFileTypes;

        public ElevationService(
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<ElevationService> logger)
        {
            _memoryCache = memoryCache;
            _configuration = configuration;
            _logger = logger;

            _dataPath = _configuration["AppSettings:DemFolder"]!;
            _demFileTypes = _configuration["AppSettings:DemFileTypes"]!.Split(",");
        }

        private DigitalElevationModelType? LookupElevations(List<IGeoLocation> points)
        {
            if (_demFileTypes.Contains(typeof(HgtRaw).Name))
            {
                var areFilesAvailable = CheckDemFilesAvailabilityAndFillCache(points, typeof(HgtRaw));
                if (areFilesAvailable)
                {
                    foreach (var point in points)
                    {
                        var filename = HgtRaw.GetFilename(point.Latitude, point.Longitude);
                        var hgt = _memoryCache.Get<IDemFile>(filename);
                        if (hgt == null) { throw new Exception($"DEM file not in cache {filename}"); }
                        point.Elevation = hgt.GetElevation(point.Latitude, point.Longitude);
                    }
                    return DigitalElevationModelType.SRTM1;
                }
            }

            if (_demFileTypes.Contains(typeof(GeoTiff).Name))
            {
                var areFilesAvailable = CheckDemFilesAvailabilityAndFillCache(points, typeof(GeoTiff));
                if (areFilesAvailable)
                {
                    foreach (var point in points)
                    {
                        var filename = GeoTiff.GetFilename(point.Latitude, point.Longitude);
                        var hgt = _memoryCache.Get<IDemFile>(filename);
                        if (hgt == null) { throw new Exception($"DEM file not in cache {filename}"); }
                        point.Elevation = hgt.GetElevation(point.Latitude, point.Longitude);
                    }
                    return DigitalElevationModelType.SRTM3;
                }
            }

            if (_demFileTypes.Contains(typeof(HgtPng).Name))
            {
                var areFilesAvailable = CheckDemFilesAvailabilityAndFillCache(points, typeof(HgtPng));
                if (areFilesAvailable)
                {
                    foreach (var point in points)
                    {
                        var filename = HgtPng.GetFilename(point.Latitude, point.Longitude);
                        var hgt = _memoryCache.Get<IDemFile>(filename);
                        if (hgt == null) { throw new Exception($"DEM file not in cache {filename}"); }
                        point.Elevation = hgt.GetElevation(point.Latitude, point.Longitude);
                    }
                    return DigitalElevationModelType.SRTM1;
                }
            }

            return null;
        }

        private bool CheckDemFilesAvailabilityAndFillCache(List<IGeoLocation> points, Type demFileType)
        {
            var areFilesAvailable = true;
            var filenames = new List<string>();
            var missingFilenames = new List<string>();
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
                if (File.Exists(Path.Combine(_dataPath, filename)))
                {
                    if (!filenames.Any(x => x == filename))
                    {
                        filenames.Add(filename);
                    }
                }
                else
                {
                    areFilesAvailable = false;

                    if (!missingFilenames.Any(x => x == filename))
                    {
                        missingFilenames.Add(filename);
                    }
                }
            }

            foreach (var filename in missingFilenames)
            {
                //_logger.LogDebug($"DEM file missing: {filename}");
            }

            if (areFilesAvailable)
            {
                var demFilesCacheInfo = new List<(string Filename, long FileSize, bool IsAlreadyCached)>();
                foreach (var filename in filenames)
                {
                    if (_memoryCache.TryGetValue(filename, out _))
                    {
                        demFilesCacheInfo.Add((filename, 0, true));
                    }
                    else
                    {
                        demFilesCacheInfo.Add((filename, new FileInfo(Path.Combine(_dataPath, filename)).Length, false));
                    }
                }

                var size = ((MemoryCache)_memoryCache).GetSize();
                var maxSize = ((MemoryCache)_memoryCache).GetMaxSize();
                var entryInfos = ((MemoryCache)_memoryCache).GetEntryInfos();
                if (size + demFilesCacheInfo.Where(x => x.IsAlreadyCached == false).Sum(x => x.FileSize) > maxSize)
                {
                    _logger.LogWarning("Cache doesn't have enough space to add all items");
                    foreach (var entryInfo in entryInfos)
                    {
                        if (!demFilesCacheInfo.Any(x => x.IsAlreadyCached == true && x.Filename == entryInfo.Key))
                        {
                            _logger.LogInformation($"Removing DEM file from cache: {entryInfo.Key}");
                            _memoryCache.Remove(entryInfo.Key);
                        }
                        size = ((MemoryCache)_memoryCache).GetSize();
                        if (size + demFilesCacheInfo.Where(x => x.IsAlreadyCached == false).Sum(x => x.FileSize) < maxSize)
                        {
                            break;
                        }
                    }
                }

                foreach (var demFileCacheInfo in demFilesCacheInfo.Where(x => x.IsAlreadyCached == false))
                {
                    _logger.LogInformation($"Adding DEM file to cache: {demFileCacheInfo.Filename}");

                    using (var stream = new FileStream(Path.Combine(_dataPath, demFileCacheInfo.Filename), FileMode.Open, FileAccess.Read))
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
                        _memoryCache.Set(demFileCacheInfo.Filename, demFile, new MemoryCacheEntryOptions()
                        {
                            Size = demFile.SizeInBytes
                        });
                    }
                }
                if (demFilesCacheInfo.Any(x => x.IsAlreadyCached == false))
                {
                    size = ((MemoryCache)_memoryCache).GetSize();
                    entryInfos = ((MemoryCache)_memoryCache).GetEntryInfos();
                    _logger.LogInformation($"Cache statistics, Total size: {size}, Max size: {maxSize}");
                    foreach (var info in entryInfos)
                    {
                        _logger.LogInformation($"Cache statistics, Key: {info.Key}, Size: {info.Size}");
                    }
                }
            }

            return areFilesAvailable;
        }

        public DigitalElevationModelType LookupElevations(List<IGeoLocation> points, SmoothingMode smoothingMode, int maxPoints)
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
                            ElevationProfileSmoothingHelper.WindowSmooth(points, [0.1f, 1f, 0.1f]);
                            break;
                        case SmoothingMode.FeedbackSmooth:
                            ElevationProfileSmoothingHelper.FeedbackSmooth(points, 2f, 0.65f);
                            break;
                    }
                }
                else
                {
                    switch (smoothingMode)
                    {
                        case SmoothingMode.WindowSmooth:
                            ElevationProfileSmoothingHelper.WindowSmooth(points, [1, 2, 3, 2, 1]);
                            break;
                        case SmoothingMode.FeedbackSmooth:
                            ElevationProfileSmoothingHelper.FeedbackSmooth(points, 3, 1);
                            break;
                    }
                }

                return demType.Value;
            }
            else
            {
                return DigitalElevationModelType.None;
            }
        }
    }
}
