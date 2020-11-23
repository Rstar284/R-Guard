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

namespace RGuard
{
    public class Program
    {
        public readonly EventId BotEventId = new EventId(42, "R Graud");
        private static readonly DiscordConfiguration _clientConfig = new DiscordConfiguration
        {
            Intents = DiscordIntents.All,
            MessageCacheSize = 4096,
            MinimumLogLevel = LogLevel.Error
        };

        public static async Task Main(string[] args) => await CreateHostBuilder(args).RunConsoleAsync().ConfigureAwait(false);

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
.UseConsoleLifetime()
.ConfigureAppConfiguration((context, configuration) =>
{
    configuration.SetBasePath(Directory.GetCurrentDirectory());
    configuration.AddJsonFile("appSettings.json", false, false);
    configuration.AddUserSecrets(typeof(Program).Assembly, optional: true, reloadOnChange: false);
})
.ConfigureServices((context, services) =>
{
    IConfiguration config = context.Configuration;
    _clientConfig.Token = config.GetConnectionString("BotToken");
    services.AddSingleton(new DiscordShardedClient(_clientConfig));
    services.AddDbContextFactory<BotDbContext>(option => option.UseNpgsql(config.GetConnectionString("dbConnection")), ServiceLifetime.Transient);
    services.AddMemoryCache(option => option.ExpirationScanFrequency = TimeSpan.FromHours(1));

    services.AddSingleton<PrefixCacheService>();
    services.AddSingleton<GuildConfigCacheService>();
    services.AddSingleton<SerilogLoggerFactory>();
    services.AddSingleton<InfractionService>();
    services.AddSingleton<MessageCreationHandler>();
    services.AddSingleton(typeof(HttpClient), services =>
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("R Guard Project by Rstar284 / v1.0");
        return client;
    });
    services.AddSingleton<BotEventHelper>();

    services.AddHostedService<Bot>();
})
.UseSerilog();
    }
}