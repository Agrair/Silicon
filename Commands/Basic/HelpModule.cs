using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Commands.Commons;
using Silicon.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silicon.Commands.Basic
{
    [Name("help")]
    [Ratelimit(5, 10)]
    public class HelpModule : SiliconModule
    {
        public InteractiveService Interactive { get; set; }

        public CommandService Commands { get; set; }
        public IServiceProvider Services { get; set; }

        [Command("guildinfo")]
        [Summary("Gets info on the server.")]
        public async Task GuildInfo()
        {
            var g = Context.Guild;
            await ReplyAsync(new EmbedBuilder()
                .WithTitle($"{Context.Guild.Name} Info - Silicon")
                .AddField("Guild ID", g.Id)
                .AddField("Population", g.MemberCount)
                .AddField("Channel count", g.Channels.Count)
                .AddField("Role count", g.Roles.Count)
                .AddField("Birthday", g.CreatedAt)
                .AddField("Maker", $"<@{g.Owner.Id}>")
                .AddField("Age", DateTimeOffset.UtcNow.Subtract(g.CreatedAt).Normalize())
                .WithColor(Color.Gold).Build());
        }

        [Command("userinfo")]
        [Summary("Gets info on a user.")]
        public async Task UserInfo(SocketGuildUser user)
        {
            var builder = new EmbedBuilder()
                .WithTitle($"{user.Nickname ?? user.Username}#{user.Discriminator} ({(user.IsBot ? "Bot" : "User")}) {(user.IsMuted ? "(Muted)" : "")} Info - Silicon")
                .WithFooter(user.Status.ToString());
            if (user.JoinedAt.HasValue)
                builder.AddField("Joined at", user.JoinedAt?.ToUniversalTime());
            builder.AddField("Using Discord since", user.CreatedAt.ToUniversalTime());
            if (user.VoiceChannel != null)
                builder.AddField("Voice channel", user.VoiceChannel.Name);
            builder.AddField("Role count", user.Roles.Count);
            builder.AddField("Guild hierarchy pos", user.Hierarchy);
            builder.WithImageUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
            await ReplyAsync(builder.Build());
        }

        private Dictionary<string, string> modules;

        //unoptimized but whatever
        [Command("help")]
        public async Task Help([Remainder] string target = null)
        {
            if (modules == null)
            {
                modules = new Dictionary<string, string>();
                foreach (var module in Commands.Modules)
                {
                    if (module.Parent != null)
                        continue;
                    DescribeFullModule(module);
                }
            }
            if (target == null)
            {
                await Interactive.SendPaginatedMessageAsync(Context,
                    new Models.PaginationData("Modules", new Color(0xff0066), modules.Values.ToList()));
            }
            else
            {
                var args = target.Split(' ');
                int index = 0;
                foreach (var module in Commands.Modules)
                {
                    if (TryFindModule(module, args, ref index, out var result))
                    {
                        await ReplyAsync(new EmbedBuilder()
                            .WithTitle(target)
                            .WithDescription(modules[result.UniqueName()])
                            .WithColor(new Color(0xff0066))
                            .Build());
                        return;
                    }
                }
                await ReplyAsync("Could not find module.");
            }
        }

        private void DescribeFullModule(ModuleInfo module)
        {
            modules.Add(module.UniqueName(), DescribeSingleModule(module));
            foreach (var sub in module.Submodules)
                DescribeFullModule(sub);
        }

        private string DescribeSingleModule(ModuleInfo module)
        {
            var builder = new StringBuilder();
            var commands = QualifiedCommands(module);
            if (commands.Count == 0)
                return null;
            builder.AppendLine(module.Name.ToUpper().Bold());
            builder.AppendLine(prefixes(module) + GetSubmodules(module));
            foreach (var cmd in commands)
            {
                GetCommand(cmd, out var name, out var text);
                builder.AppendLine();
                builder.AppendLine(name);
                builder.AppendLine(text);
            }
            return builder.ToString();


            static string prefixes(ModuleInfo module)
            {
                return !module.Aliases.First().IsNullOrWhitespace()
                    ? $"Prefix{(module.Aliases.Count() > 1 ? "es" : "")}: {string.Join(",", module.Aliases)}\n"
                    : "";
            }
        }

        private static bool TryFindModule(ModuleInfo query, string[] arr, ref int index, out ModuleInfo result)
        {
            result = null;
            if (query.Name.EqualsIgnoreCase(arr[index]))
            {
                result = query;
                if (++index == arr.Length)
                {
                    return true;
                }
                foreach (var sub in query.Submodules)
                {
                    if (TryFindModule(sub, arr, ref index, out result))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetSubmodules(ModuleInfo module)
        {
            var submodules = module.Submodules.Where(x => QualifiedCommands(x).Count() > 0);
            return module.Submodules.Any()
                ? $"Submodule{(module.Submodules.Count() > 1 ? "s" : "")}: " +
                $"{string.Join(", ", module.Submodules.Select(m => m.Name.ToUpper()))}\n"
                : "";
        }

        private IReadOnlyList<CommandInfo> QualifiedCommands(ModuleInfo module) => (Context.IsPrivate
                ? module.Commands
                : module.GetExecutableCommandsAsync(Context, Services).GetAwaiter().GetResult())
                .Where(x => x.Module.Name.EqualsIgnoreCase(module.Name)).ToList();

        /*
        public void GetCommands(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var command in QualifiedCommands(module))
            {
                GetCommand(command, out var name, out var text);
                builder.AddField(name, text);
            }
        }
        */

        private void GetCommand(CommandInfo command, out string name, out string text)
        {
            name = command.Name.Bold();
            text = $"{command.Summary ?? "No summary available"}\n{remarks(command)}{aliases(command)}" +
                $"**Usage:** `s|{GetPrefix(command)} {GetParams(command)}`";

            static string remarks(CommandInfo command)
            {
                return !command.Remarks.IsNullOrWhitespace() ? $"({command.Remarks})\n" : "";
            }

            static string aliases(CommandInfo command)
            {
                return command.Aliases.Any()
                    ? $"**Aliases:** {string.Join(", ", command.Aliases.Select(x => x.Highlight()))}\n"
                    : "";
            }
        }

        private static string GetParams(CommandInfo command)
        {
            StringBuilder output = new StringBuilder();
            if (!command.Parameters.Any())
                return output.ToString();
            foreach (var param in command.Parameters)
            {
                if (param.IsOptional)
                    output.Append($"[{param.Name} = {param.DefaultValue ?? "NaN"}] ");
                else if (param.IsMultiple)
                    output.Append($"|{param.Name}| ");
                else if (param.IsRemainder)
                    output.Append($"..{param.Name} ");
                else
                    output.Append($"<{param.Name}> ");
            }
            return output.ToString();
        }

        private static string GetPrefix(CommandInfo command)
        {
            var output = GetPrefix(command.Module);
            output += command.Aliases.FirstOrDefault();
            return output;
        }

        private static string GetPrefix(ModuleInfo module)
        {
            string output = "";
            if (module.Parent != null)
                output = $"{GetPrefix(module.Parent)}{output}";
            return output;
        }
    }
}
