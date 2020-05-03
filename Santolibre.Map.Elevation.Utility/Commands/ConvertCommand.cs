using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Santolibre.Map.Elevation.Lib;

namespace Santolibre.Map.Elevation.Utility.Commands
{
    public class ConvertCommand
    {
        private readonly IFileConverter _fileConverter;

        public static void Configure(CommandLineApplication command)
        {
            command.Description = "Convert dem files to image formats";
            command.HelpOption("-?|-h|--help");
            var inputFormat = command.Option<InputFormat>("-if|--input-format <INPUT_FORMAT>", "[Required] Input format", CommandOptionType.SingleValue).IsRequired();
            var inputPath = command.Option("-ip|--input-path <INPUT_PATH>", "[Required] Input path", CommandOptionType.SingleValue);
            var outputFormat = command.Option<OutputFormat>("-of|--output-format <OUTPUT_FORMAT>", "[Required] Output format", CommandOptionType.SingleValue).IsRequired();
            var outputPath = command.Option("-op|--output-path <OUTPUT_PATH>", "[Required] Output path", CommandOptionType.SingleValue);

            command.OnExecute(() =>
            {
                command.GetRequiredService<ConvertCommand>().Run(inputFormat.ParsedValue, inputPath.Value(), outputFormat.ParsedValue, outputPath.Value());
                return 0;
            });
        }

        public ConvertCommand(IFileConverter fileConverter)
        {
            _fileConverter = fileConverter;
        }

        public void Run(InputFormat inputFormat, string inputPath, OutputFormat outputFormat, string outputPath)
        {
            _fileConverter.Convert(inputFormat, inputPath, outputFormat, outputPath);
        }
    }
}
