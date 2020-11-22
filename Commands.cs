using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using RGuard.Extensions;

namespace RGuard.Commands
{
    public class CommandsUngroupped : BaseCommandModule
    {
        [Command("userinfo")]
        public async Task UserInfo(CommandContext ctx, DiscordMember member)
        {
            var nickname = member.Nickname;
            await ctx.TriggerTypingAsync();
            if(member.Nickname == null)
            {
                nickname = "The member Does **NOT** have a nickname";
                var avatar = member.AvatarUrl;
                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Info on user {member.Username}")
                    .WithImageUrl(avatar)
                    .WithDescription($"Nickname in this server: {nickname}\nId: {member.Id}\nJoined this server: {member.JoinedAt}\nPermissions: {member.PermissionsIn(ctx.Channel)}\nIs A bot: {member.IsBot}\nIs owner of {member.Guild.Name}: {member.IsOwner}");
                await ctx.Channel.SendMessageAsync(embed: embed);

            }
            else
            {
                var avatar = member.GetAvatarUrl(ImageFormat.Auto);
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Info on user {member.Username}",
                    ImageUrl = avatar,
                    Description = $"Nickname in this server: {nickname}\nId: {member.Id}\nJoined this server: {member.JoinedAt}\nPermissions in this channel: {member.PermissionsIn(ctx.Channel).ToPermissionString()}\nIs A bot: {member.IsBot}\nIs owner of {member.Guild.Name}: {member.IsOwner}"
                };
                await ctx.Channel.SendMessageAsync(embed: embed);
            }
        }
        [Command("ping")] // let's define this method as a command
        [Description("Starter ping command and used for checking ping of the bot")] // this will be displayed to tell users what this command does when they invoke help
        [Aliases("pong")] // alternative names for the command
        public async Task Ping(CommandContext ctx) // this command takes no arguments
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // let's make the message a bit more colourful
            var emoji = DiscordEmoji.FromName(ctx.Client, ":ping_pong:");

            // respond with ping
            await ctx.RespondAsync($"{emoji} Pong! Ping: {ctx.Client.Ping}ms");
        }

        [Command("greet"), Description("Says hi to specified user."), Aliases("sayhi", "say_hi")]
        public async Task Greet(CommandContext ctx, [Description("The user to say hi to.")] DiscordMember member) // this command takes a member as an argument; you can pass one by username, nickname, id, or mention
        {
            // note the [Description] attribute on the argument.
            // this will appear when people invoke help for the
            // command.

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // let's make the message a bit more colourful
            var emoji = DiscordEmoji.FromName(ctx.Client, ":wave:");

            // and finally, let's respond and greet the user.
            await ctx.RespondAsync($"{emoji} Hello, {member.Mention}!");
        }

        [Command("sum"), Description("Sums all given numbers and returns said sum.")]
        public async Task SumOfNumbers(CommandContext ctx, [Description("Integers to sum.")] params int[] args)
        {
            // note the params on the argument. It will indicate
            // that the command will capture all the remaining arguments
            // into a single array

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // calculate the sum
            var sum = args.Sum();

            // and send it to the user
            await ctx.RespondAsync($"The sum of these numbers is {sum:#,##0}");
        }

        // this command will use our custom type, for which have 
        // registered a converter during initialization
        [Command("math"), Description("Does basic math.")]
        public async Task Math(CommandContext ctx, [Description("Operation to perform on the operands.")] MathOperation operation, [Description("First operand.")] double num1, [Description("Second operand.")] double num2)
        {
            var result = 0.0;
            switch (operation)
            {
                case MathOperation.Add:
                    result = num1 + num2;
                    break;

                case MathOperation.Subtract:
                    result = num1 - num2;
                    break;

                case MathOperation.Multiply:
                    result = num1 * num2;
                    break;

                case MathOperation.Divide:
                    result = num1 / num2;
                    break;

                case MathOperation.Modulo:
                    result = num1 % num2;
                    break;
            }

            var emoji = DiscordEmoji.FromName(ctx.Client, ":1234:");
            _ = await ctx.RespondAsync($"{emoji} The result is {result:#,##0.00}");
        }
    }

    [Group("admin")] // let's mark this class as a command group
    [Description("Administrative commands.")] // give it a description for help purposes
    [Hidden] // let's hide this from the eyes of curious users
    [RequirePermissions(Permissions.ManageGuild)] // and restrict this to users who have appropriate permissions
    public class GrouppedCommands : BaseCommandModule
    {
        // all the commands will need to be executed as <prefix>admin <command> <arguments>

        // this command will be only executable by the bot's owner
        [Command("sudo"), Description("Executes a command as another user."), Hidden, RequireUserPermissions(Permissions.Administrator)]
        public async Task Sudo(CommandContext ctx, [Description("Member to execute as.")] DiscordMember member, [RemainingText, Description("Command text to execute.")] string command)
        {
            // note the [RemainingText] attribute on the argument.
            // it will capture all the text passed to the command

            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            // get the command service, we need this for
            // sudo purposes
            var cmds = ctx.CommandsNext;

            // retrieve the command and its arguments from the given string
            var cmd = cmds.FindCommand(command, out var customArgs);

            // create a fake CommandContext
            var fakeContext = cmds.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, customArgs);

            // and perform the sudo
            await cmds.ExecuteCommandAsync(fakeContext);
        }

        [Command("nick"), Description("Gives someone a new nickname."), RequirePermissions(Permissions.ManageNicknames)]
        public async Task ChangeNickname(CommandContext ctx, [Description("Member to change the nickname for.")] DiscordMember member, [RemainingText, Description("The nickname to give to that user.")] string new_nickname)
        {
            // let's trigger a typing indicator to let
            // users know we're working
            await ctx.TriggerTypingAsync();

            try
            {
                // let's change the nickname, and tell the 
                // audit logs who did it.
                await member.ModifyAsync(x =>
                {
                    x.Nickname = new_nickname;
                    x.AuditLogReason = $"Changed by {ctx.User.Username} ({ctx.User.Id}).";
                });

                // let's make a simple response.
                var emoji = DiscordEmoji.FromName(ctx.Client, ":+1:");

                // and respond with it.
                await ctx.RespondAsync(emoji);
            }
            catch (Exception)
            {
                // oh no, something failed, let the invoker now
                var emoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
                await ctx.RespondAsync(emoji);
            }
        }
        [Command("kick"), Description("Kick a User")]
        public async Task Kick(CommandContext ctx, DiscordMember member,[RemainingText, Description("Why you want to kick the user")] string reason)
        {
            try
            {
                if (!ctx.Guild.CurrentMember.HasPermission(Permissions.KickMembers))
                {
                    var embed1 = new DiscordEmbedBuilder()
                        .WithTitle("I do not have the permission to kick members")
                        .WithColor(DiscordColor.Red)
                        .WithFooter("Dbot", ctx.Guild.CurrentMember.AvatarUrl);
                    await ctx.RespondAsync(embed: embed1);
                    return;
                }
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Member Kicked")
                    .AddField("Who Got Kicked?", member.Username)
                    .WithDescription("The user has been informed in DMs")
                    .AddField("Reason", reason ?? "no reason specified")
                    .WithFooter("Dbot", ctx.Guild.CurrentMember.AvatarUrl);
                await ctx.Channel.SendMessageAsync(embed: embed);
                await member.SendMessageAsync($"You were Kicked from {ctx.Guild.Name} beacuse {reason}");
                await member.RemoveAsync(reason ?? "no reason specified");
            }
            catch(NotFoundException)
            {
                await ctx.Channel.SendMessageAsync($"ERROR!, {member.Username} Could not be found!");
            }
            
        }
    }
}