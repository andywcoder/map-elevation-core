﻿using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;

namespace Santolibre.Map.Elevation.Lib
{
    public class MetadataService : IMetadataService
    {
        private readonly IConfiguration _configuration;

        public MetadataService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private List<SrtmRectangle> GetHGTRectangles(int fileSize)
        {
            var rectangles = new List<SrtmRectangle>();
            var dataPath = _configuration["AppSettings:DemFolder"];

            if (Directory.Exists(dataPath))
            {
                var files = Directory.GetFiles(dataPath, "*.hgt");
                foreach (var file in files)
                {
                    var filename = file.Substring(file.LastIndexOf('\\') + 1).ToUpper();
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.Length == fileSize)
                    {
                        var latDir = filename.Substring(0, 1);
                        var minLat = int.Parse(filename.Substring(1, 2));
                        var lonDir = filename.Substring(3, 1);
                        var minLon = int.Parse(filename.Substring(4, 3));
                        if (latDir == "S")
                            minLat *= -1;
                        if (lonDir == "W")
                            minLon *= -1;
                        rectangles.Add(new SrtmRectangle { FileFormat = "hgt", Resolution = fileSize == HgtRaw.HGT3601 ? "1 arc-second" : "3 arc-seconds", Left = minLon, Right = minLon + 1, Bottom = minLat, Top = minLat + 1 });
                    }
                }
            }

            return rectangles;
        }

        private List<SrtmRectangle> GetGeoTiffRectangles()
        {
            var rectangles = new List<SrtmRectangle>();
            var dataPath = _configuration["AppSettings:DemFolder"];

            if (Directory.Exists(dataPath))
            {
                var files = Directory.GetFiles(dataPath, "*.tif");
                foreach (var file in files)
                {
                    var filename = file.Substring(file.LastIndexOf('\\') + 1);
                    var latIndex = int.Parse(filename.Substring(8, 2));
                    var lonIndex = int.Parse(filename.Substring(5, 2));
                    var lat = 60 - (latIndex * 5);
                    var lon = (lonIndex - 37) * 5;
                    rectangles.Add(new SrtmRectangle { FileFormat = "geotiff", Resolution = "3 arc-second", Left = lon, Right = lon + 5, Bottom = lat, Top = lat + 5 });
                }
            }

            return rectangles;
        }

        public List<SrtmRectangle> GetSRTM1Rectangles()
        {
            return GetHGTRectangles(HgtRaw.HGT3601);
        }

        public List<SrtmRectangle> GetSRTM3Rectangles()
        {
            var rectangles = GetGeoTiffRectangles();
            rectangles.AddRange(GetHGTRectangles(HgtRaw.HGT1201));
            return rectangles;
        }
    }
}
