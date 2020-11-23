using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RGuard.Database;
using RGuard.Database.Models;

namespace RGuard.Extras
{
    public class BotEventHelper
    {

        private readonly IDbContextFactory<BotDbContext> _dbFactory;
        private readonly ILogger<BotEventHelper> _logger;
        private readonly DiscordShardedClient _client;
        private readonly Stopwatch _time = new();
        private volatile bool _logged = false;
        private int _currentMemberCount = 0;
        private readonly int expectedMembers;
        private readonly int cachedMembers;


        public static List<Action> CacheStaff { get; } = new();
        public static Task GuildDownloadTask { get; private set; } = new(() => Task.Delay(-1));
        public object BotEvents { get; private set; }

        public BotEventHelper(DiscordShardedClient client, IDbContextFactory<BotDbContext> dbFactory, ILogger<BotEventHelper> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _client = client;
            _logger.LogInformation("Created Event Helper");
        }
        public void CreateHandlers()
        {

            _client.ClientErrored += (c, e) =>
            {
                e.Handled = true;
                if (e.Exception.Message.Contains("event handler")) _logger.LogError(e.Exception.Message + " " + e.EventName);//_logger.LogWarning($"An event handler timed out. [{e.EventName}]");
                else _logger.LogError($"An exception was thrown; message: {e.Exception.Message}");
                return Task.CompletedTask;
            };
            _client.GuildAvailable += Cache;
        }



        private Task Cache(DiscordClient c, GuildCreateEventArgs e)
        {
            if (!_time.IsRunning)
            {
                _time.Start();
                _logger.LogTrace("Beginning Cache Run...");
            }
            _ = Task.Run(async () =>
            {
                using var db = _dbFactory.CreateDbContext();
                var sw = Stopwatch.StartNew();
                Guild guild = db.Guilds.AsQueryable().Include(g => g.Users).FirstOrDefault(g => g.Id == e.Guild.Id);
                sw.Stop();
                _logger.LogTrace($"Retrieved guild from database in {sw.ElapsedMilliseconds} ms; guild {(guild is not null ? "does" : "does not")} exist.");

                if (guild is null)
                {
                    guild = new Guild { Id = e.Guild.Id, Prefix = Bot.DefaultCommandPrefix };
                    db.Guilds.Add(guild);
                }

                sw.Restart();
                await db.SaveChangesAsync();

                sw.Stop();
                if (sw.ElapsedMilliseconds > 400) _logger.LogWarning($"Query took longer than allocated [250ms] time with tolerance of [150ms]. Query time: [{sw.ElapsedMilliseconds} ms]");
                _logger.LogDebug($"Shard [{c.ShardId + 1}/{c.ShardCount}] | Guild [{++_currentMemberCount}/{c.Guilds.Count}] | Time [{sw.ElapsedMilliseconds}ms]");
                if (_currentMemberCount == c.Guilds.Count && !_logged)
                {
                    _logged = true;
                    _time.Stop();
                    _logger.LogTrace("Cache run complete.");
                    _logger.LogDebug($"Expected [{expectedMembers}] members to be cached got [{cachedMembers}] instead. Cache run took {_time.ElapsedMilliseconds} ms.");
                }
            });
            return Task.CompletedTask;
        }
    }
}