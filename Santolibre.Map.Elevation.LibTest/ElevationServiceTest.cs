using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKCoolsoft.GPXLib;
using Moq;
using Santolibre.Map.Elevation.Lib;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Santolibre.Map.Elevation.LibTest
{
    [TestClass]
    [DeploymentItem("TestData/N46E008.zip")]
    [DeploymentItem("TestData/srtm_44_06.zip")]
    public class ElevationServiceTest
    {
        [ClassInitialize]
        public static void ExtractDemData(TestContext testContext)
        {
            using (var zipFile = ZipFile.OpenRead(Path.Combine("TestData", "N46E008.zip")))
            {
                zipFile.ExtractToDirectory("TestData", true);
            }
            using (var zipFile = ZipFile.OpenRead(Path.Combine("TestData", "srtm_44_06.zip")))
            {
                zipFile.ExtractToDirectory("TestData", true);
            }
        }

        private List<IGeoPoint> ReadPoints(string filename)
        {
            var gpx = new GPXLib();
            gpx.LoadFromFile(filename);
            return gpx.TrkList.First().TrksegList.First().TrkptList.ConvertAll(x => (IGeoPoint)new GeoPoint() { Latitude = (float)x.Lat, Longitude = (float)x.Lon });
        }

        private Statistics CalculateStatistics(List<IGeoPoint> points)
        {
            var statistics = new Statistics();

            for (var j = 1; j < points.Count; j++)
            {
                if (points[j - 1].Elevation.Value < points[j].Elevation.Value)
                    statistics.Gain += points[j].Elevation.Value - points[j - 1].Elevation.Value;
                else
                    statistics.Loss += points[j - 1].Elevation.Value - points[j].Elevation.Value;
            }

            statistics.Minimum = points.Min(x => x.Elevation.Value);
            statistics.Maximum = points.Max(x => x.Elevation.Value);

            return statistics;
        }

        [TestMethod]
        [DeploymentItem("TestData/elevation_profile_1.gpx")]
        public void LookupElevations_PointsNoSmoothing_SRTM1()
        {
            // Arrange
            var maxPoints = 10000;
            var cacheService = new Mock<IDemFileCache>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            configuration.SetupKeyValuePair("AppSettings:DemFileTypes", "GeoTiff,HgtRaw,HgtPng");
            var points = ReadPoints(Path.Combine("TestData", "elevation_profile_1.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.LookupElevations(points.ConvertAll(x => (IGeoLocation)x), SmoothingMode.None, maxPoints);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM1, digitalElevationModelType);
            var statistics = CalculateStatistics(points);
            Assert.AreEqual(3215, statistics.Gain);
            Assert.AreEqual(3139, statistics.Loss);
        }

        [TestMethod]
        [DeploymentItem("TestData/elevation_profile_1.gpx")]
        public void LookupElevations_PointsWindowSmooth_SRTM1()
        {
            // Arrange
            var maxPoints = 10000;
            var cacheService = new Mock<IDemFileCache>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            configuration.SetupKeyValuePair("AppSettings:DemFileTypes", "GeoTiff,HgtRaw,HgtPng");
            var points = ReadPoints(Path.Combine("TestData", "elevation_profile_1.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.LookupElevations(points.ConvertAll(x => (IGeoLocation)x), SmoothingMode.WindowSmooth, maxPoints);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM1, digitalElevationModelType);
            var statistics = CalculateStatistics(points);
            Assert.AreEqual(2920, (int)statistics.Gain);
            Assert.AreEqual(2844, (int)statistics.Loss);
        }

        [TestMethod]
        [DeploymentItem("TestData/elevation_profile_1.gpx")]
        public void LookupElevations_PointsFeedbackSmooth_SRTM1()
        {
            // Arrange
            var maxPoints = 10000;
            var cacheService = new Mock<IDemFileCache>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            configuration.SetupKeyValuePair("AppSettings:DemFileTypes", "GeoTiff,HgtRaw,HgtPng");
            var points = ReadPoints(Path.Combine("TestData", "elevation_profile_1.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.LookupElevations(points.ConvertAll(x => (IGeoLocation)x), SmoothingMode.FeedbackSmooth, maxPoints);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM1, digitalElevationModelType);
            var statistics = CalculateStatistics(points);
            Assert.AreEqual(2505, (int)statistics.Gain);
            Assert.AreEqual(2428, (int)statistics.Loss);
        }

        [TestMethod]
        [DeploymentItem("TestData/elevation_below_sea_level_route.gpx")]
        public void LookupElevations_PointsWithBelowSeaLevelElevationsNoSmoothing_SRTM3()
        {
            // Arrange
            var maxPoints = 10000;
            var cacheService = new Mock<IDemFileCache>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            configuration.SetupKeyValuePair("AppSettings:DemFileTypes", "GeoTiff,HgtRaw,HgtPng");
            var points = ReadPoints(Path.Combine("TestData", "elevation_below_sea_level_route.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.LookupElevations(points.ConvertAll(x => (IGeoLocation)x), SmoothingMode.None, maxPoints);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM3, digitalElevationModelType);
            var statistics = CalculateStatistics(points);
            Assert.AreEqual(5244, statistics.Gain);
            Assert.AreEqual(5244, statistics.Loss);
            Assert.AreEqual(-244, statistics.Minimum);
            Assert.AreEqual(295, statistics.Maximum);
        }
    }
}
