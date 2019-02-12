using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MKCoolsoft.GPXLib;
using Moq;
using Santolibre.Map.Elevation.Lib.Models;
using Santolibre.Map.Elevation.Lib.Services;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Santolibre.Map.Elevation.LibTest.Services
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

        private List<INode> ReadNodes(string filename)
        {
            var gpx = new GPXLib();
            gpx.LoadFromFile(filename);
            return gpx.TrkList.First().TrksegList.First().TrkptList.ConvertAll(x => (INode)new Node() { Latitude = (float)x.Lat, Longitude = (float)x.Lon });
        }

        private Statistics CalculateStatistics(List<INode> nodes)
        {
            var statistics = new Statistics();

            for (var j = 1; j < nodes.Count; j++)
            {
                if (nodes[j - 1].Elevation < nodes[j].Elevation)
                    statistics.Gain += nodes[j].Elevation - nodes[j - 1].Elevation;
                else
                    statistics.Loss += nodes[j - 1].Elevation - nodes[j].Elevation;
            }

            statistics.Minimum = nodes.Min(x => x.Elevation);
            statistics.Maximum = nodes.Max(x => x.Elevation);

            return statistics;
        }

        [TestMethod]
        [DeploymentItem("TestData/elevation_profile_1.gpx")]
        public void GetElevations_NodesNoSmoothing_SRTM1()
        {
            // Arrange
            var maxNodes = 10000;
            var cacheService = new Mock<ICacheService>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            var nodes = ReadNodes(Path.Combine("TestData", "elevation_profile_1.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.GetElevations(nodes, SmoothingMode.None, maxNodes);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM1, digitalElevationModelType);
            var statistics = CalculateStatistics(nodes);
            Assert.AreEqual(3215, statistics.Gain);
            Assert.AreEqual(3139, statistics.Loss);
        }



        [TestMethod]
        [DeploymentItem("TestData/elevation_profile_1.gpx")]
        public void GetElevations_NodesWindowSmooth_SRTM1()
        {
            // Arrange
            var maxNodes = 10000;
            var cacheService = new Mock<ICacheService>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            var nodes = ReadNodes(Path.Combine("TestData", "elevation_profile_1.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.GetElevations(nodes, SmoothingMode.WindowSmooth, maxNodes);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM1, digitalElevationModelType);
            var statistics = CalculateStatistics(nodes);
            Assert.AreEqual(2920, (int)statistics.Gain);
            Assert.AreEqual(2844, (int)statistics.Loss);
        }

        [TestMethod]
        [DeploymentItem("TestData/elevation_profile_1.gpx")]
        public void GetElevations_NodesFeedbackSmooth_SRTM1()
        {
            // Arrange
            var maxNodes = 10000;
            var cacheService = new Mock<ICacheService>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            var nodes = ReadNodes(Path.Combine("TestData", "elevation_profile_1.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.GetElevations(nodes, SmoothingMode.FeedbackSmooth, maxNodes);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM1, digitalElevationModelType);
            var statistics = CalculateStatistics(nodes);
            Assert.AreEqual(2505, (int)statistics.Gain);
            Assert.AreEqual(2428, (int)statistics.Loss);
        }

        [TestMethod]
        [DeploymentItem("TestData/elevation_below_sea_level_route.gpx")]
        public void GetElevations_NodesWithBelowSeaLevelElevationsNoSmoothing_SRTM3()
        {
            // Arrange
            var maxNodes = 10000;
            var cacheService = new Mock<ICacheService>();
            var configuration = new Mock<IConfiguration>();
            configuration.SetupKeyValuePair("AppSettings:DemFolder", "TestData");
            var nodes = ReadNodes(Path.Combine("TestData", "elevation_below_sea_level_route.gpx"));

            var elevationService = new ElevationService(cacheService.Object, configuration.Object);

            // Act
            var digitalElevationModelType = elevationService.GetElevations(nodes, SmoothingMode.None, maxNodes);

            // Assert
            Assert.AreEqual(DigitalElevationModelType.SRTM3, digitalElevationModelType);
            var statistics = CalculateStatistics(nodes);
            Assert.AreEqual(5244, statistics.Gain);
            Assert.AreEqual(5244, statistics.Loss);
            Assert.AreEqual(-244, statistics.Minimum);
            Assert.AreEqual(295, statistics.Maximum);
        }
    }
}
