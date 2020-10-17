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

        private readonly Dictionary<ulong, TriviaGame> _games;

        private string dbToken;

        private int correct;
        private string title;
        private string[] choices;
        private DateTimeOffset timeOfQuestion;
        private readonly Stack<TriviaQuestion> _questions;

        private static readonly char[] _letters = { 'A', 'B', 'C', 'D' };

        public TriviaService(HttpClient http)
        {
            _client = http;
            _rand = new Random();

            dbToken = RequestToken();
            _questions = new Stack<TriviaQuestion>();
            _games = new Dictionary<ulong, TriviaGame>();
        }


        public void SetChannel(ulong guild, SocketGuildChannel channel)
        {
            if (_games.TryGetValue(guild, out var game))
            {
                game.Channel = channel;
                game.Timer.Change(1_000, -1);
            }
            else
            {
                game = new TriviaGame(channel, new Timer(TimerCallback, game, 15_000, Timeout.Infinite));
                _games.Add(guild, game);
            }
        }

        public bool StopTrivia(ulong guild)
        {
            if (_games.TryGetValue(guild, out var game))
                return false;

            game.Timer.Dispose();
            _games.Remove(guild);
            return true;
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
                _questions.Push(JsonConvert.DeserializeObject<TriviaQuestion>(result.ToString()));
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
            var guild = (msg.Author as SocketGuildUser).Guild;
            if (!_games.TryGetValue(guild.Id, out var game) || msg.Channel.Id != game.Channel.Id)
                return false;

            var content = msg.Content;
            if (content.EqualsIgnoreCase(choices[correct])
                || (content.Length == 1 && Array.IndexOf(_letters, content.ToUpper()[0]) == correct))
            {
                var channel = msg.Channel as SocketGuildChannel;
                await channel.AddPermissionOverwriteAsync(guild.EveryoneRole,
                    OverwritePermissions.InheritAll.Modify(sendMessages: PermValue.Deny));
                game.Timer.Change(15_000, -1);

                await msg.Channel.SendMessageAsync(embed: GetEmbed(msg.Author).Build());
                return true;
            }

            else
            {
                await msg.AddReactionAsync(wrong);
                return false;
            }
        }

        private async void TimerCallback(object? state)
        {
            var (channel, timer) = (state as TriviaGame).AsTuple;

            if (_questions.Count == 0)
            {
                await GetQAAsync();
            }
            var q = _questions.Pop();
            var builder = new EmbedBuilder()
                .WithTitle(title = $"{q.Category.ToUpper()}, difficulty: {q.Difficulty.ToUpper()}")
                .WithDescription(q.Question.DecodeHtml());
            var choices = new List<string>(q.FalseAnswers);
            if (q.Type == "multiple")
            {
                var index = (byte)_rand.Next(0, choices.Count);
                choices.Insert(index, q.Answer);
                correct = index;
            }
            else
            {
                if (q.FalseAnswers[0] == "True")
                {
                    correct = 1;
                    choices.Add("False");
                }
                else
                {
                    correct = 0;
                    choices.Insert(correct, "True");
                }
            }
            this.choices = choices.ToArray();
            for (byte i = 0; i < choices.Count; i++)
            {
                string choice = choices[i];
                builder.AddField(new EmbedFieldBuilder()
                    .WithName(Convert.ToString(_letters[i]))
                    .WithValue(choice.DecodeHtml()));
            }
            builder.WithFooter("Made with `opentdb.com`");

            await (channel as ISocketMessageChannel).SendMessageAsync(embed: builder.Build());
            timeOfQuestion = DateTimeOffset.Now;
            await channel.AddPermissionOverwriteAsync(channel.Guild.EveryoneRole,
                OverwritePermissions.InheritAll.Modify(sendMessages: PermValue.Allow));

            timer.Change(-1, -1);
        }

        private EmbedBuilder GetEmbed(IUser user)
        {
            return new EmbedBuilder()
                .WithTitle(title)
                .WithDescription("Correct!")
                .WithFooter($"Answered by {user.FullName()} in {DateTimeOffset.Now.Subtract(timeOfQuestion)}",
                    user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
        }
    }
}
