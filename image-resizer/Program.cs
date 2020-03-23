
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SerilogTimings;
using SerilogTimings.Extensions;
using System.Threading.Tasks;

namespace ImageResizer
{
    class Program
    {
        static async Task Main()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            using ServiceProvider serviceProvider = services.BuildServiceProvider();
            //ImageResizerImageMagick app = serviceProvider.GetService<ImageResizerImageMagick>();
            AzureBlobQueueReader app = serviceProvider.GetService<AzureBlobQueueReader>();
            await app.RunAsync();
        }
        private static void ConfigureServices(ServiceCollection services)
        {
            //services.AddTransient<ImageResizerImageMagick>();
            services.AddTransient<AzureBlobQueueReader>();

            
            var serilogLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Async(a => a.File("Log.txt"))
                //.WriteTo.File("Log.txt")
                .WriteTo.Console()
                .CreateLogger();
            
            /*
            var serilogLogger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                //.WriteTo.LiterateConsole()
                .WriteTo.Console()
                //.WriteTo.Seq("http://localhost:5341")
                .CreateLogger();
            */

            Log.Logger = serilogLogger;

            services.AddLogging(builder =>
            {
                //builder.SetMinimumLevel(LogLevel.Trace);
                //builder.AddSerilog(logger: serilogLogger, dispose: true);
                builder.AddSerilog(logger: Log.Logger, dispose: true);
            });
        }
    }
}
