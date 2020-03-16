﻿
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace image_resizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                MyApplication app = serviceProvider.GetService<MyApplication>();
                app.Run();
            }
        }
        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddTransient<MyApplication>()
                    //.AddScoped<IBusinessLayer, CBusinessLayer>()
                    //.AddSingleton<IDataAccessLayer, CDataAccessLayer>();
                    ;

            var serilogLogger = new LoggerConfiguration()
            .WriteTo.File("Log.txt")
            .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddSerilog(logger: serilogLogger, dispose: true);
            });
        }
    }
}
