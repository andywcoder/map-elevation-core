using McMaster.Extensions.CommandLineUtils;

namespace Santolibre.Map.Elevation.Utility.Commands
{
    public class RootCommand
    {
        private readonly CommandLineApplication _app;

        public static void Configure(CommandLineApplication app)
        {
            app.HelpOption("-?|-h|--help");

            app.Command("convert", ConvertCommand.Configure);

            app.OnExecute(() =>
            {
                (new RootCommand(app)).Run();
                return 0;
            });
        }

        public RootCommand(CommandLineApplication app)
        {
            _app = app;
        }

        public void Run()
        {
            _app.ShowHelp();
        }
    }
}
