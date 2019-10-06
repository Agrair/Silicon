using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace Silicon.Services
{
    public class WebService
    {
        private readonly IYoutubeClient client = new YoutubeClient();

        public async Task<IReadOnlyList<Video>> SearchYoutubeAsync(string term)
        {
            return await client.SearchVideosAsync(term, 2);
        }
    }
}
