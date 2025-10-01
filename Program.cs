using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasExpo.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace EasExpo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    DbSeeder.SeedAsync(services).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    if (ex is SqlException || ex.InnerException is SqlException)
                    {
                        var sqlEx = ex as SqlException ?? ex.InnerException as SqlException;
                        logger.LogError(sqlEx, "Database seeding failed. Verify that SQL Server is running, the instance name in ConnectionStrings:DefaultConnection is correct, and the credentials have access. See README.md > SQL Server connectivity checklist for troubleshooting steps.");
                    }
                    else
                    {
                        logger.LogError(ex, "An error occurred while seeding the database.");
                    }
                    throw;
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
