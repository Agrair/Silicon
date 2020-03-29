using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Commands.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silicon.Commands.Basic
{
    [Name("help")]
    [Ratelimit(5, 10)]
    public class HelpModule : PandoraModule
    {
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
            if (user.JoinedAt.HasValue) builder.AddField("Joined at", user.JoinedAt?.ToUniversalTime());
            builder.AddField("Using Discord since", user.CreatedAt.ToUniversalTime());
            if (user.VoiceChannel != null) builder.AddField("Voice channel", user.VoiceChannel.Name);
            builder.AddField("Role count", user.Roles.Count);
            builder.AddField("Guild hierarchy pos", user.Hierarchy);
            builder.WithImageUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
            await ReplyAsync(builder.Build());
        }

        private static List<ModuleInfo> modules;

        [Command("help")]
        public async Task Help([Remainder] string target = null)
        {
            //TODO recursion for sub modules
            if (modules == null)
            {
                modules = new List<ModuleInfo>();
                foreach (var mod in Commands.Modules.Where(m => m.Parent == null))
                {
                    modules.Add(mod);
                }
            }
            foreach (var )
            {
                await Interactive.SendPaginatedMessageAsync(Context,
                    new Models.PaginationData(target.ToUpper(), new Color(0xff0066), new List<string> { text }));
            }
            else await Interactive.SendPaginatedMessageAsync(Context,
                new Models.PaginationData("Modules", new Color(0xff0066), modules.Values.ToList()));
        }

        private void DescribeModule(ModuleInfo module, ref List<ModuleInfo> list)
        {
            var builder = new StringBuilder();
            var commands = QualifiedCommands(module);
            if (commands.Count > 0)
            {
                builder.AppendLine(module.Name.ToUpper().Bold());
                builder.AppendLine(aliases(module) + GetSubmodules(module));
                foreach (var command in commands)
                {
                    GetCommand(command, out var name, out var text);
                    builder.AppendLine(name + "\n" + text + "\n");
                }
                list.Add(module.Name, builder.ToString());
                builder.Clear();
            }
            foreach (var sub in module.Submodules) DescribeModule(sub, ref list);

            static string aliases(ModuleInfo module)
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
                if (++index == arr.Length)
                {
                    result = query;
                    return true;
                }
                foreach (var sub in query.Submodules)
                {
                    if (TryFindModule(sub, arr, ref index, out var subResult))
                    {
                        result = subResult;
                        return true;
                    }
                }
            }
            return false;
        }

        public string GetSubmodules(ModuleInfo module)
        {
            var submodules = module.Submodules.Where(x => QualifiedCommands(x).Count() > 0);
            return module.Submodules.Any()
                ? $"Submodule{(module.Submodules.Count() > 1 ? "s" : "")}: " +
                $"{string.Join(", ", module.Submodules.Select(m => m.Name.ToUpper()))}\n"
                : "";
        }

        public IReadOnlyList<CommandInfo> QualifiedCommands(ModuleInfo module) => (Context.IsPrivate
                ? module.Commands
                : module.GetExecutableCommandsAsync(Context, Services).GetAwaiter().GetResult())
                .Where(x => x.Module.Name.EqualsIgnoreCase(module.Name)).ToList();

        public void GetCommands(ModuleInfo module, ref EmbedBuilder builder)
        {
            foreach (var command in QualifiedCommands(module))
            {
                GetCommand(command, out var name, out var text);
                builder.AddField(name, text);
            }
        }

        public void GetCommand(CommandInfo command, out string name, out string text)
        {
            name = command.Name.Bold();
            text = $"{command.Summary ?? "No summary available"}\n{remarks(command)}{aliases(command)}" +
                $"**Usage:** `|{GetPrefix(command)} {GetParams(command)}`";

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

        public static string GetParams(CommandInfo command)
        {
            StringBuilder output = new StringBuilder();
            if (!command.Parameters.Any()) return output.ToString();
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

        public static string GetPrefix(CommandInfo command)
        {
            var output = GetPrefix(command.Module);
            output += command.Aliases.FirstOrDefault();
            return output;
        }

        public static string GetPrefix(ModuleInfo module)
        {
            string output = "";
            if (module.Parent != null) output = $"{GetPrefix(module.Parent)}{output}";
            return output;
        }
    }
}
