using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;

namespace Silicon.Helpers
{
    static class NetHelper
    {
        private static readonly IYoutubeClient client = new YoutubeClient();

        public static async Task<IReadOnlyList<Video>> SearchYoutubeAsync(string term)
        {
            return await client.SearchVideosAsync(term, 2);
        }

        public static async Task<Stream> GetHttpStream(string url)
        {
            using var client = new HttpClient();
            return await client.GetStreamAsync(url);
        }
    }
}
