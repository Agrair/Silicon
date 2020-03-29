using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

        public PaginationData Options { get; }
        private int curPage;

        public PaginatedCallback(SocketCommandContext context,
            PaginationData options)
            : base(context)
        {
            Options = options;
            curPage = 1;
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
            var emote = reaction.Emote.Name;

            if (emote.Equals(First.Name))
                curPage = 1;
            else if (emote.Equals(Next.Name))
            {
                if (curPage >= Options.Pages.Count)
                    return false;
                ++curPage;
            }
            else if (emote.Equals(Back.Name))
            {
                if (curPage <= 1)
                    return false;
                --curPage;
            }
            else if (emote.Equals(Last.Name))
                curPage = Options.Pages.Count;
            else if (emote.Equals(Stop.Name))
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
