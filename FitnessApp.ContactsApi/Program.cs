using System;
using System.Threading.Tasks;
using ContactsApi.Data;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace FitnessApp.ContactsApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == Microsoft.Extensions.Hosting.Environments.Development)
            {
                //await DataInitializer.EnsureContactsAreCreatedAsync(host.Services);                
            }
            await host.RunAsync();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
        }
    }
}