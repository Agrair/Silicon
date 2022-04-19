using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Silicon.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Silicon.Services
{
    public class TriviaService
    {
        private readonly HttpClient _client;
        private readonly Random _rand;

        private readonly ConcurrentDictionary<ulong, TriviaGame> _games;

        private string dbToken;

        private readonly Stack<TriviaQuestion> _questions;

        private static readonly char[] _letters = { 'A', 'B', 'C', 'D' };

        public TriviaService(HttpClient http)
        {
            _client = http;
            _rand = new Random();

            dbToken = RequestToken();
            _questions = new Stack<TriviaQuestion>();
            _games = new ConcurrentDictionary<ulong, TriviaGame>();
        }


        public void SetChannel(ulong guild, SocketTextChannel channel)
        {
            if (_games.TryGetValue(guild, out var game))
            {
                game.Channel = channel;
                game.Timer.Change(1_000, -1);
            }
            else
            {
                game = new TriviaGame {
                    Channel = channel
                };

                game.Timer = new Timer(PostQuestion, game, 2_000, Timeout.Infinite);

                _games.TryAdd(guild, game);
            }
        }

        public bool StopTrivia(ulong guild)
        {
            if (_games.TryRemove(guild, out var game)) {
                game.Timer.Dispose();
                return true;
            }

            return false;
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
            var content = await GetContent(dbToken);
            var jObj = JObject.Parse(content);

            if (jObj["response_code"].ToString() == "4")
            {
                dbToken = RequestToken();
                content = await GetContent(dbToken);
                jObj = JObject.Parse(content);
            }

            var jArr = JArray.Parse(jObj["results"].ToString());
            foreach (var result in jArr)
            {
                _questions.Push(JsonConvert.DeserializeObject<TriviaQuestion>(result.ToString()));
            }

            Task<string> GetContent(string token)
            {
                return _client.GetStringAsync($"https://opentdb.com/api.php?amount=10&token={token}");
            }
        }


        private static readonly IEmote wrong = new Emoji("👎");
        public async Task CheckAnswer(SocketUserMessage msg)
        {
            if (!(msg.Author is SocketGuildUser user))
                return;

            if (!_games.TryGetValue(user.Guild.Id, out var game) || msg.Channel.Id != game.Channel.Id)
                return;

            var content = msg.Content.ToLower();
            if (content.EqualsIgnoreCase(game.Choices[game.Correct])
                || (content.Length == 1 && Array.IndexOf(_letters, content.ToUpper()[0]) == game.Correct))
            {
                await game.Channel.AddPermissionOverwriteAsync(user.Guild.EveryoneRole,
                    OverwritePermissions.InheritAll.Modify(sendMessages: PermValue.Deny));
                game.Timer.Change(15_000, -1);

                await game.Channel.SendMessageAsync(embed: GetEmbed(msg.Author).Build());
            }

            else
            {
                await msg.AddReactionAsync(wrong);
            }

            EmbedBuilder GetEmbed(IUser user) {
                return new EmbedBuilder()
                    .WithTitle(game.Title)
                    .WithDescription("Correct!")
                    .WithFooter($"Answered by {user.FullName()} in {DateTimeOffset.Now.Subtract(game.Timestamp).Seconds}s",
                        user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl());
            }
        }

        private async void PostQuestion(object? state)
        {
            var game = state as TriviaGame;

            if (_questions.Count == 0)
            {
                await GetQAAsync();
            }
            var q = _questions.Pop();
            var builder = new EmbedBuilder()
                .WithTitle(game.Title = $"{q.Category.ToUpper()}, difficulty: {q.Difficulty.ToUpper()}")
                .WithDescription(q.Question.DecodeHtml());
            var choices = new List<string>(q.FalseAnswers);
            if (q.Type == "multiple")
            {
                var index = (byte)_rand.Next(0, choices.Count);
                choices.Insert(index, q.Answer);
                game.Correct = index;
            }
            else
            {
                if (q.FalseAnswers[0] == "True")
                {
                    game.Correct = 1;
                    choices.Add("False");
                }
                else
                {
                    game.Correct = 0;
                    choices.Insert(game.Correct, "True");
                }
            }

            for (byte i = 0; i < choices.Count; i++)
            {
                string choice = choices[i];
                builder.AddField(new EmbedFieldBuilder()
                    .WithName(Convert.ToString(_letters[i]))
                    .WithValue(choice.DecodeHtml()));
            }
            builder.WithFooter("Made with `opentdb.com`");

            game.Choices = choices.ToArray();
            game.Timer.Change(-1, -1);

            await game.Channel.SendMessageAsync(embed: builder.Build());
            game.Timestamp = DateTimeOffset.Now;
            await game.Channel.AddPermissionOverwriteAsync(game.Channel.Guild.EveryoneRole,
                OverwritePermissions.InheritAll.Modify(sendMessages: PermValue.Allow));
        }
    }
}
