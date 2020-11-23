using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RGuard.Database;
using RGuard.Database.Models;

namespace RGuard.Services
{
    public class GuildConfigCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IDbContextFactory<BotDbContext> _dbFactory;
        private readonly ILogger<GuildConfigCacheService> _logger;

        public GuildConfigCacheService(IMemoryCache cache, IDbContextFactory<BotDbContext> dbFactory, ILogger<GuildConfigCacheService> logger)
        {
            _cache = cache;
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public async ValueTask<GuildConfiguration> GetConfigAsync(ulong? guildId)
        {
            if (guildId is null || guildId == 0) return default;
            else if (_cache.TryGetValue(guildId.Value, out GuildConfiguration config)) return config;
            else return await GetConfigFromDatabaseAsync(guildId.Value);
        }

        public async ValueTask<GuildConfiguration> GetConfigFromDatabaseAsync(ulong guildId)
        {
            var db = _dbFactory.CreateDbContext();
            Guild config = await db.Guilds.AsNoTracking().FirstAsync(g => g.Id == guildId);
            if (config is null)
            {
                _logger.LogError("Expected value 'Guild' from databse, received null isntead.");
                return default;
            }
            var guildConfig = new GuildConfiguration(config.BlacklistWords, config.GreetMembers, config.MuteRoleId, config.GreetingChannel);
            _cache.CreateEntry(guildId).SetValue(guildConfig).SetPriority(CacheItemPriority.Low);
            return guildConfig;
        }

    }

    public struct GuildConfiguration
    {
        public bool WordBlacklistEnabled { get; set; }
        public ulong? MuteRoleId { get; set; }
        public ulong? GreetingChannel { get; set; }

        public List<BlackListedWord> BlacklistedWords { get; }

        public GuildConfiguration
            (
            bool wordBlacklistEnabled = default,
            bool greetMembers = false,
            ulong? muteRoleId = default,
            ulong? greetingChannel = default
            )
        {
            WordBlacklistEnabled = wordBlacklistEnabled;
            MuteRoleId = muteRoleId;
            GreetingChannel = greetingChannel;
            BlacklistedWords = new List<BlackListedWord>();
        }
    }

}