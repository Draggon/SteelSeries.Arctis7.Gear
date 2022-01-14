using System;
using System.Threading;
using System.Windows.Forms;
using Serilog;

namespace SteelSeries.Arctis7.Gear
{
    public class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            Serilog.Core.Logger logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(@"Logs\log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            logger.Information("Application started");

            var app = new FormsApp(logger);

            if (!app.Exit)
            {
                Application.Run();
            }

            while (!app.Exit)
            {
                Thread.Sleep(500);
            }

            logger.Information("Application stopped");
        }
    }
}