using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Services;
using System.Threading.Tasks;

namespace Silicon.Models.Callbacks
{
    public class PaginatedCallback : ReactionCallback
    {
        private readonly IEmote First = new Emoji("⏮");
        private readonly IEmote Back = new Emoji("◀");
        private readonly IEmote Next = new Emoji("▶");
        private readonly IEmote Last = new Emoji("⏭");
        private readonly IEmote Stop = new Emoji("⏹");

        public PaginatedOptions Options { get; }
        private int curPage;

        public PaginatedCallback(SocketCommandContext context,
            InteractiveService reaction,
            PaginatedOptions options)
            : base(context, reaction)
        {
            Options = options;
            curPage = options.Pages.Count;
        }

        public async Task SendAsync()
        {
            Embed embed = BuildEmbed();
            Message = await FirstContext.Channel.SendMessageAsync(embed: embed);
            _ = Task.Run(async () =>
            {
                await Message.AddReactionAsync(First);
                await Message.AddReactionAsync(Back);
                await Message.AddReactionAsync(Next);
                await Message.AddReactionAsync(Last);
                await Message.AddReactionAsync(Stop);
            });
        }

        public override async Task<bool> ExecuteAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(First))
                curPage = 1;
            else if (emote.Equals(Next))
            {
                if (curPage >= Options.Pages.Count)
                    return false;
                ++curPage;
            }
            else if (emote.Equals(Back))
            {
                if (curPage <= 1)
                    return false;
                --curPage;
            }
            else if (emote.Equals(Last))
                curPage = Options.Pages.Count;
            else if (emote.Equals(Stop))
            {
                await Message.DeleteAsync();
                return true;
            }
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await ReRenderAsync();
            return false;
        }

        private Embed BuildEmbed()
        {
            return new EmbedBuilder()
                .WithDescription(Options.Pages[curPage - 1])
                .WithFooter($"Page {curPage}/{Options.Pages.Count}")
                .WithTitle(Options.Name).Build();
        }

        private async Task ReRenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed);
        }
    }
}
