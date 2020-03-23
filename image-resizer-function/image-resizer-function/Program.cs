using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ImageResizerFunction
{
    public class Program
    {
        //public static void Main(string[] args)

        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
               .Build();

            await CreateHostBuilder(args).Build().RunAsync();
            //await host.RunAsync();
        }
        /*
        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        */

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
