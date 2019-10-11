using Discord;
using Discord.Commands;
using Silicon.Commands.Commons;
using Silicon.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode.Models;

namespace Silicon.Commands.Basic
{
    [Group("web")]
    [Ratelimit(5, 10)]
    public class NetModule : PandoraModule
    {
        public InteractiveService Reaction { get; set; }

        [Command("yt", RunMode = RunMode.Async)]
        [Summary("Searches on YouTube.")]
        public async Task YT([Remainder] string search = "Terraria")
        {
            var searchResults = await Helpers.NetHelper.SearchYoutubeAsync(search);
            var lists = searchResults.DivideList(5);
            var pages = new List<string>();

            foreach (var l in lists)
            {
                var builder = new StringBuilder();
                foreach (var video in l)
                {
                    builder.AppendLine(
                        //video title
                        $"[{video.Title}]" +
                        //video link
                        $"(https://www.youtube.com/watch?v={video.Id})" +
                        //desc and video creator
                        $"\n{desc(video)} by {video.Author}\n"
                        .Replace("&#39;", "'"));
                }
                pages.Add(builder.ToString());
            }

            await Reaction.SendPaginatedMessageAsync(Context,
                new Models.PaginatedOptions("YouTube", new Color(0xff0000), pages));

            static string desc(Video query)
            {
                string desc = query.Description.Substring(0, query.Description.Length.Clamp(0, 80));
                if (query.Description.Length > 80) desc += "...";
                return desc;
            }
        }
    }
}
