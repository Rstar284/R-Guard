using DSharpPlus;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;

namespace RGuard.Services
{
    public class MessageCreationHandler
    {
        public async Task OnMessageCreate(DiscordClient sender, MessageCreateEventArgs e)
        {
            if(e.MentionedUsers.Count > 5)
            {
                await e.Message.DeleteAsync("Mass ping");
                await e.Channel.SendMessageAsync($"{e.Author} has been warned for mass ping");
            }
        }
    }
}