using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MapApp
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

            //List<Path> paths = new List<Path>();
            //paths.AddRange(GetWayBetweenCities("1", new List<string>() { "Kyiv", "Svitlovodsk" }));
            //paths.AddRange(GetWayBetweenCities("2", new List<string>() { "Svitlovodsk", "Kharkiv" }));
            //paths.AddRange(GetWayBetweenCities("3", new List<string>() { "Kharkiv", "Kyiv", "Svitlovodsk" }));
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
