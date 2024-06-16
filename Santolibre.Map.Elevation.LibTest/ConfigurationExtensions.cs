using Microsoft.Extensions.Configuration;
using Moq;

namespace Santolibre.Map.Elevation.LibTest
{
    public static class ConfigurationHelper
    {
        public static void SetupKeyValuePair(this Mock<IConfiguration> configuration, string key, string value)
        {
            configuration.Setup(x => x[key]).Returns(value);
        }
    }
}
