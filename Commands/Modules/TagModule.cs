using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Models;
using Silicon.Modules.Commons;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Silicon.Commands.Modules
{
    [Group("tag")]
    public class TagModule : PandoraModule
    {
        public Services.TagService Tags { get; set; }

        [Name("~get")]
        [Command]
        [Priority(-1)]
        [Summary("Gets a phrase.")]
        public async Task GetPhrase(string name)
        {
            if (Tags.TryGetTag(name, Context.User, out Tag value))
            {
                var builder = new EmbedBuilder()
                    .WithDescription(value.Text)
                    .WithTitle(name);
                if (value.Color != -1) builder.WithColor(new Color((uint)value.Color));
                if (value.Claimed) builder.WithAuthor(Context.Client.GetUser(value.Owner));
                await ReplyAsync(builder.Build());
            }
            else await ReplyAsync("Phrase not found for user " + Context.User);
        }

        [Name("~get")]
        [Command]
        [Priority(-1)]
        [Summary("Gets a phrase.")]
        public async Task GetPhrase(string name, SocketGuildUser user)
        {
            if (Tags.TryGetTag(name, user, out Tag value))
            {
                var builder = new EmbedBuilder()
                    .WithDescription(value.Text)
                    .WithTitle(name);
                if (value.Color != -1) builder.WithColor(new Color((uint)value.Color));
                if (value.Claimed) builder.WithAuthor(Context.Client.GetUser(value.Owner));
                await ReplyAsync(builder.Build());
            }
            else await ReplyAsync("Phrase not found for " + user);
        }

        [Name("phrase management")]
        [BotChannelsOnly]
        public class ManagementModule : PandoraModule
        {
            public Services.TagService Phrase { get; set; }

            [Command("set")]
            [Alias("-s")]
            [Summary("Sets one of your phrases.")]
            public async Task SetPhrase(string name, [Remainder] string content)
            {
                Phrase.SetTag(name, content, Context.User);
                await ReplyAsync("Successfully set phrase");
            }

            [Command("set")]
            [Alias("-s")]
            [Summary("Sets one of your phrases.")]
            public async Task SetPhrase(string name, Color embedHue, [Remainder] string content)
            {
                Phrase.SetTag(name, content, Context.User, embedHue);
                await ReplyAsync("Successfully set phrase");
            }

            //TODO: |phrase edit

            [Command("transer")]
            [Summary("Transfers ownership of one your phrases to someone else.")]
            public async Task Transfer(string name, SocketGuildUser newOwner)
            {
                if (Phrase.TryTransfer(Context.User, name, newOwner))
                {
                    await ReplyAsync("Successfully transfered");
                }
                await ReplyAsync("Failed to transfer");
            }

            [Command("remove")]
            [Alias("-r")]
            [Summary("Deletes one of your phrases.")]
            public async Task RemovePhrase(string name)
            {
                if (Phrase.TryRemoveTag(name, Context.User))
                {
                    await ReplyAsync("Successfully removed phrase");
                }
                else await ReplyAsync("Phrase not found");
            }

            [Command("find")]
            [Alias("search")]
            [Summary("Searches for a phrase containing the follow text.")]
            public async Task FindPhrases([Remainder] string search)
            {
                if (Phrase.TryFindTag(search, out List<Tag> list))
                {
                    int count = list.Count.Clamp(0, 10);
                    var bldr = new StringBuilder();
                    for (int i = 0; i < count; i++)
                    {
                        Tag phrase = list[i];
                        bldr.AppendLine($"{i + 1}. {Context.Client.GetUser(phrase.Owner).Mention}: {phrase.Name} " +
                            $"`|phrase {phrase.Name} {phrase.Owner}`");
                    }
                    await ReplyAsync(new EmbedBuilder()
                        .WithTitle($"Showing {count}/{list.Count} phrases")
                        .WithDescription(bldr.ToString()).Build());
                }
                else await ReplyAsync("No tags found");
            }

            [Command("list")]
            [Summary("Displays one page of phrases.")]
            public async Task ListPhrases(int page = 1)
            {
                page -= 1;
                var phrases = Phrase.GetPhrases();
                var bldr = new StringBuilder();
                var pageCount = (int)Math.Ceiling(phrases.Count * .1);
                page = page.Clamp(0, pageCount);
                var bottom = ((page + 1) * 10).Clamp(10, phrases.Count);
                for (int i = page * 10; i < bottom; i++)
                {
                    Tag phrase = phrases[i];
                    bldr.AppendLine($"{i + 1}. {Context.Client.GetUser(phrase.Owner).Mention}: {phrase.Name} " +
                        $"`|phrase {phrase.Name} {phrase.Owner}`");
                }
                await ReplyAsync(new EmbedBuilder()
                    .WithTitle($"Page {page + 1}/{pageCount}")
                    .WithDescription(bldr.ToString()).Build());
            }

            [Group("admin")]
            [RequireUserPermission(ChannelPermission.ManageMessages)]
            public class AdminModule : PandoraModule
            {
                public Services.TagService Phrase { get; set; }

                [Command("set")]
                [Alias("-s")]
                [Summary("Admin set.")]
                public async Task SetPhrase(string name, IUser user, [Remainder] string content)
                {
                    Phrase.SetTag(name, content, user);
                    await ReplyAsync("Successfully set phrase");
                }

                [Command("remove")]
                [Alias("-r")]
                [Summary("Admin remove.")]
                public async Task RemovePhrase(string name, IUser user)
                {
                    if (Phrase.TryRemoveTag(name, user))
                    {
                        await ReplyAsync("Successfully removed phrase");
                    }
                    else await ReplyAsync("Phrase not found");
                }
            }
        }
    }
}