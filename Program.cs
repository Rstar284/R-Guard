using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace RGuard
{
    public class Program
    {
        private static readonly DiscordConfiguration _clientConfig = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            MessageCacheSize = 4096,
            MinimumLogLevel = LogLevel.Error
        };

        public static async Task Main(string[] args) => await CreateHostBuilder(args).RunConsoleAsync().ConfigureAwait(false);


        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
.UseConsoleLifetime()
.ConfigureAppConfiguration((context, configuration) =>
{
configuration.SetBasePath(Directory.GetCurrentDirectory());
configuration.AddJsonFile("appSettings.json", false, false);
configuration.AddUserSecrets(typeof(Program).Assembly, optional: true, reloadOnChange: false);
})
.ConfigureLogging((context, builder) => Log.Logger = new LoggerConfiguration()
.WriteTo.Console(outputTemplate: "[{Timestamp:h:mm:ss-ff tt}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
.MinimumLevel.Verbose()
.CreateLogger())
.ConfigureServices((context, services) =>
{
IConfiguration config = context.Configuration;
_clientConfig.Token = config.GetConnectionString("BotToken");
services.AddSingleton(typeof(HttpClient), services =>
{
var client = new System.Net.Http.HttpClient();
client.DefaultRequestHeaders.UserAgent.ParseAdd("R Guard Bot/Project v1.0 by Rstar284");
return client;
});
})
.UseSerilog();
        }
    }
}