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
using RGuard.Database;
using RGuard.Extras;
using RGuard.Services;
using Serilog.Extensions.Logging;
using Serilog;
using Serilog.Sinks.Console;

namespace RGuard
{
    public class Program
    {
        private static readonly DiscordConfiguration _clientConfig = new()
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
                           configuration.AddJsonFile("appSettings.json", true, false);
                           configuration.AddUserSecrets<Program>(true, false);
                       })
                       .ConfigureLogging((context, builder) => Log.Logger = new LoggerConfiguration()
                                                                            .WriteTo.Console(
                                                                                outputTemplate:
                                                                                "[{Timestamp:h:mm:ss-ff tt}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                                                                            .MinimumLevel.Override("Microsoft",
                                                                                LogEventLevel.Warning)
                                                                            .MinimumLevel.Verbose()
                                                                            .CreateLogger())
                       .ConfigureServices((context, services) =>
                       {
                           IConfiguration config = context.Configuration;
                           _clientConfig.Token = config.GetConnectionString("BotToken");
                           services.AddSingleton(new DiscordShardedClient(_clientConfig));
                           services.AddDbContextFactory<BotDbContext>(
                               option => option.UseNpgsql(config.GetConnectionString("dbConnection")),
                               ServiceLifetime.Transient);
                           services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromHours(1));

                           services.AddSingleton<PrefixCacheService>();
                           services.AddSingleton<GuildConfigCacheService>();
                           services.AddSingleton<SerilogLoggerFactory>();
                           services.AddSingleton<InfractionService>();
                           services.AddSingleton<MessageCreationHandler>();
                           services.AddSingleton(typeof(HttpClient), _ =>
                           {
                               var client = new HttpClient();
                               client.DefaultRequestHeaders.UserAgent.ParseAdd("R Guard by Rstar284 / v1.0");
                               return client;
                           });
                           services.AddScoped(_ => new BotConfig(config));
                           services.AddSingleton<BotEventHelper>();

                           services.AddHostedService<Bot>();
                       })
                       .UseSerilog();
        }
    }
}