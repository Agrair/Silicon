using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silicon.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Silicon.Services
{
    public class TriviaService
    {
        private readonly HttpClient _client;
        private readonly Random _rand;

        public SocketGuildChannel Channel { get; private set; }

        private string dbToken;
        private readonly Timer _timer;

        private char correctAnswer;
        private readonly Stack<TriviaQuestion> questions;

        private readonly Dictionary<byte, char> numbersToLetters;

        public TriviaService(HttpClient http)
        {
            _client = http;
            _rand = new Random();
            _timer = new Timer(async _ =>
            {
                if (questions.Count == 0)
                {
                    await GetQAAsync();
                }
                var q = questions.Pop();
                var builder = new EmbedBuilder();
                builder.WithTitle($"{q.Category.ToUpper()}, difficulty: {q.Difficulty.ToUpper()}");
                builder.WithDescription(q.Question.DecodeHtml());
                var choices = new List<string>(q.FalseAnswers);
                var index = (byte)_rand.Next(0, choices.Count);
                choices.Insert(index, q.Answer);
                correctAnswer = numbersToLetters[index];
                for (byte i = 0; i < choices.Count; i++)
                {
                    string choice = choices[i];
                    builder.AddField(new EmbedFieldBuilder()
                        .WithName(Convert.ToString(numbersToLetters[i]))
                        .WithValue(choice.DecodeHtml()));
                }
                builder.WithFooter("Made with `opentdb.com`");
                if (Channel is ISocketMessageChannel msgChannel)
                {
                    await msgChannel.SendMessageAsync(embed: builder.Build());
                }
                await Channel.AddPermissionOverwriteAsync(Channel.Guild.EveryoneRole,
                    OverwritePermissions.InheritAll.Modify(sendMessages: PermValue.Allow));
                _timer.Change(-1, -1);
            }, null, -1, -1);

            dbToken = RequestToken();
            questions = new Stack<TriviaQuestion>();
            numbersToLetters = new Dictionary<byte, char>
            {
                { 0, 'A' },
                { 1, 'B' },
                { 2, 'C' },
                { 3, 'D' }
            };
        }

        public void SetChannel(SocketGuildChannel channel)
        {
            Channel = channel;
            _timer.Change(1_000, -1);
        }

        public void StopTrivia()
        {
            Channel = null;
            _timer.Change(-1, -1);
        }

        private string RequestToken()
        {
            var content = _client.GetAsync("https://opentdb.com/api_token.php?command=request")
                .Result.Content;
            var obj = JObject.Parse(content.ReadAsStringAsync().Result);
            return obj["token"].ToString();
        }

        private async Task GetQAAsync()
        {
            var content = await GetContent();
            var jObj = JObject.Parse(content);

            if (jObj["response_code"].ToString() == "4")
            {
                dbToken = RequestToken();
                content = await GetContent();
                jObj = JObject.Parse(content);
            }

            var jArr = JArray.Parse(jObj["results"].ToString());
            foreach (var result in jArr)
            {
                questions.Push(JsonConvert.DeserializeObject<TriviaQuestion>(result.ToString()));
            }

            async Task<string> GetContent()
            {
                return await (await _client.GetAsync($"https://opentdb.com/api.php?amount=10&token={dbToken}"))
                    .Content.ReadAsStringAsync();
            }
        }

        private static readonly IEmote wrong = new Emoji("👎");
        public async Task<bool> CheckAnswer(SocketUserMessage msg)
        {
            if (msg.Content[0].ToString().EqualsIgnoreCase(correctAnswer.ToString()))
            {
                await Channel.AddPermissionOverwriteAsync(Channel.Guild.EveryoneRole,
                    OverwritePermissions.InheritAll.Modify(sendMessages: PermValue.Deny));
                _timer.Change(15_000, -1);
                return true;
            }
            else
            {
                await msg.AddReactionAsync(wrong);
                return false;
            }
        }
    }
}
