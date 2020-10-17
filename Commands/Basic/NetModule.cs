using Discord;
using Discord.Commands;
using Silicon.Commands.Commons;
using Silicon.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace Silicon.Commands.Basic
{
    [Group("net")]
    [Ratelimit(5, 10)]
    public class NetModule : SiliconModule
    {
        private static readonly YoutubeClient yt = new YoutubeClient();
        public InteractiveService Reaction { get; set; }

        [Command("yt", RunMode = RunMode.Async)]
        [Summary("Searches on YouTube.")]
        public async Task YT([Remainder] string search = "Terraria")
        {
            var pages = new List<string>();

            await foreach (var video in yt.Search.GetVideosAsync(search, 0, 2))
            {
                pages.Add(
                    //video title
                    $"[{video.Title}]" +
                    //video link
                    $"(https://www.youtube.com/watch?v={video.Id})" +
                    //desc and video creator
                    $"\n{desc(video)} by {video.Author}\n"
                    .Replace("&#39;", "'"));
            }

            await Reaction.SendPaginatedMessageAsync(Context,
                new Models.PaginationData("YouTube", new Color(0xff0000), pages));

            static string desc(Video query)
            {
                string desc = query.Description.Substring(0, query.Description.Length.Clamp(0, 80));
                if (query.Description.Length > 80)
                    desc += "...";
                return desc;
            }
        }
    }
}
