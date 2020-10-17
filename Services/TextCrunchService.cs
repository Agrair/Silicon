using Discord;
using Discord.WebSocket;
using Silicon.Models.Enums;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Silicon.Services
{
    //https://github.com/tModLoader/tModLoader-Discord-Bot/blob/rework/Services/HastebinService.cs
    public class TextCrunchService : IDisposable
    {
        private readonly HttpClient client;

        private static readonly Regex HastebinRegex = new Regex(@"{""key"":""(?<key>[a-z].*)""}",
            RegexOptions.Compiled);

        private readonly string[] CodeBlockTypes = new string[]
        {
            "html",
            "css",
            "cs",
            "dns",
            "python",
            "lua",
            "http",
            "markdown",
        };

        private readonly char[] CodeKeyChars = new char[]
        {
            '{',
            '}',
            '=',
            ';',
            '<',
            '>',
            '(',
            ')',
        };

        private string site;

        public TextCrunchService()
        {
            site = ChooseSite();
        }

        public bool OfflineCheck(out string site)
        {
            OfflineCheck();
            return (site = this.site) != null;
        }

        private bool OfflineCheck()
        {
            site = ChooseSite();
            return site != null;
        }

        public async Task TryHaste(SocketUserMessage message)
        {
            string contents = message.Content;
            bool shouldHastebin = false;
            string extra = "";

            var attachment = message.Attachments.First();
            if (attachment != null)
            {
                if (attachment.Filename.EndsWith(".log") && attachment.Size < 100000)
                {
                    contents = await client.GetStringAsync(attachment.Url);

                    shouldHastebin = true;
                    extra = $" `({attachment.Filename})`";
                }
            }

            if (string.IsNullOrWhiteSpace(contents))
                return;

            shouldHastebin = contents.Where(c => CodeKeyChars.Contains(c)).Count() > 6
                && message.Content.Split('\n').Length >= 8;

            if (shouldHastebin)
            {
                if (!OfflineCheck())
                    return;

                string hastebinContent = contents.Trim('`');
                for (int i = 0; i < CodeBlockTypes.Length; i++)
                {
                    string keyword = CodeBlockTypes[i];
                    if (hastebinContent.StartsWith(keyword + "\n"))
                    {
                        hastebinContent = hastebinContent.Substring(keyword.Length).TrimStart('\n');
                        break;
                    }
                }

                var msg = await message.Channel.SendMessageAsync("Text-crunching in progress...");

                HttpContent content = new StringContent(hastebinContent);

                var response = await client.PostAsync(site + "/documents", content);
                string resultContent = await response.Content.ReadAsStringAsync();

                var match = HastebinRegex.Match(resultContent);

                if (!match.Success)
                    return;

                string hasteUrl = $"{site}/{match.Groups["key"]}";
                await msg.ModifyAsync(x => x.Content = $"Automatic Hastebin for {message.Author.Username}" +
                    $"{extra}: {hasteUrl}");

                await message.DeleteAsync();
                await Helpers.LoggingHelper.Log(LogSeverity.Verbose, LogSource.Silicon, $"Hasted message {msg.Id} by {msg.Author.Id} in {msg.Channel.Id}");
            }
        }

        private string ChooseSite()
        {
            try
            {
                using var ping = new Ping();
                var result = ping.Send("paste.mod.gg");
                if (result.Status == IPStatus.Success)
                    return "https://paste.mod.gg";
                result = ping.Send("hastebin.com");
                if (result.Status == IPStatus.Success)
                    return "https://hastebin.com";
            }
            catch (PingException e) { Helpers.LoggingHelper.Log(LogSeverity.Warning, LogSource.Service, null, e); }

            return null;
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}