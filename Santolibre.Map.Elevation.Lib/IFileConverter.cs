namespace Santolibre.Map.Elevation.Lib
{
    public interface IFileConverter
    {
        void Convert(InputFormat inputFormat, string inputPath, OutputFormat outputFormat, string outputPath);
    }
}
