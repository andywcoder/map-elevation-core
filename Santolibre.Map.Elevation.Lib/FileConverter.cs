using System;

namespace Santolibre.Map.Elevation.Lib
{
    public class FileConverter : IFileConverter
    {
        public void Convert(InputFormat inputFormat, string inputPath, OutputFormat outputFormat, string outputPath)
        {
            IDemFile inputDemFile;
            switch (inputFormat)
            {
                case InputFormat.HGT:
                    inputDemFile = HgtRaw.Create(inputPath);
                    break;
                case InputFormat.PNG:
                    inputDemFile = HgtPng.Create(inputPath);
                    break;
                default:
                    throw new Exception("Unsupported input format");
            }

            IDemFile outputDemFile;
            switch (outputFormat)
            {
                case OutputFormat.HGT:
                    outputDemFile = HgtRaw.Create(inputDemFile.Data);
                    break;
                case OutputFormat.PNG:
                    outputDemFile = HgtPng.Create(inputDemFile.Data);
                    break;
                default:
                    throw new Exception("Unsupported output format");
            }
            outputDemFile.Save(outputPath);
        }
    }
}
