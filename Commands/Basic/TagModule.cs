using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Silicon.Commands.Commons;
using Silicon.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Silicon.Commands.Basic
{
    [Group("tag")]
    [Ratelimit(5, 5)]
    public class TagModule : PandoraModule
    {
        public Services.TagService Tags { get; set; }

        [Name("~get")]
        [Command]
        [Priority(-1)]
        [Summary("Gets a tag.")]
        public async Task GetTag(string name)
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
            else await ReplyAsync($"Tag not found for user {Context.User}.");
        }

        [Name("~get")]
        [Command]
        [Priority(-1)]
        [Summary("Gets a tag.")]
        public async Task GetTag(string name, SocketGuildUser user)
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
            else await ReplyAsync($"Tag not found for {user}.");
        }

        [Command("find")]
        [Alias("search")]
        [Summary("Searches for a tag containing the follow text.")]
        public async Task FindTags([Remainder] string search)
        {
            if (Tags.TryFindTag(search, out List<Tag> list))
            {
                int count = list.Count.Clamp(0, 10);
                var bldr = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    Tag tag = list[i];
                    bldr.AppendLine($"{i + 1}. {Context.Client.GetUser(tag.Owner).Mention}: {tag.Name} " +
                        $"`|tag {tag.Name} {tag.Owner}`");
                }
                await ReplyAsync(new EmbedBuilder()
                    .WithTitle($"Showing {count}/{list.Count} tags")
                    .WithDescription(bldr.ToString()).Build());
            }
            else await ReplyAsync("No tags found.");
        }

        [Command("list")]
        [Summary("Displays one page of tags.")]
        public async Task ListTags(int page = 1)
        {
            page -= 1;
            var bldr = new StringBuilder();
            var pageCount = (int)Math.Ceiling(Tags.Tags.Count * .1);
            page = page.Clamp(0, pageCount);
            var bottom = ((page + 1) * 10).Clamp(10, Tags.Tags.Count);
            for (int i = page * 10; i < bottom; i++)
            {
                Tag tag = Tags.Tags[i];
                bldr.AppendLine($"{i + 1}. {Context.Client.GetUser(tag.Owner).Mention}: {tag.Name} " +
                    $"`|tag {tag.Name} {tag.Owner}`");
            }
            await ReplyAsync(new EmbedBuilder()
                .WithTitle($"Page {page + 1}/{pageCount}")
                .WithDescription(bldr.ToString()).Build());
        }

        [Name("tag management")]
#pragma warning disable CA1034 // Nested types should not be visible
        public class ManagementModule : PandoraModule
        {
            public Services.TagService Tag { get; set; }

            [Command("set")]
            [Alias("-s")]
            [Summary("Sets one of your tags.")]
            public async Task SetTag(string name, [Remainder] string content)
            {
                Tag.SetTag(name, content, Context.User);
                await ReplyAsync("Successfully set tag");
            }

            [Command("set")]
            [Alias("-s")]
            [Summary("Sets one of your tags.")]
            public async Task SetTag(string name, Color embedHue, [Remainder] string content)
            {
                Tag.SetTag(name, content, Context.User, embedHue);
                await ReplyAsync("Successfully set tag");
            }

            //TODO: |tag edit

            [Command("transer")]
            [Summary("Transfers ownership of one your tags to someone else.")]
            public async Task Transfer(string name, SocketGuildUser newOwner)
            {
                if (Tag.TryTransfer(Context.User, name, newOwner))
                {
                    await ReplyAsync("Successfully transfered tag.");
                }
                await ReplyAsync("Could not find tag.");
            }

            [Command("remove")]
            [Alias("-r")]
            [Summary("Deletes one of your tags.")]
            public async Task RemoveTag(string name)
            {
                if (Tag.TryRemoveTag(name, Context.User))
                {
                    await ReplyAsync("Successfully removed tag.");
                }
                else await ReplyAsync("Tag not found");
            }

            [Group("admin")]
            [RequireUserPermission(ChannelPermission.ManageMessages)]
            public class AdminModule : PandoraModule
            {
#pragma warning restore CA1034 // Nested types should not be visible
                public Services.TagService Tag { get; set; }

                [Command("set")]
                [Alias("-s")]
                [Summary("Admin set.")]
                public async Task SetTag(string name, SocketGuildUser user, [Remainder] string content)
                {
                    Tag.SetTag(name, content, user);
                    await ReplyAsync("Successfully set tag.");
                }

                [Command("remove")]
                [Alias("-r")]
                [Summary("Admin remove.")]
                public async Task RemoveTag(string name, SocketGuildUser user)
                {
                    if (Tag.TryRemoveTag(name, user))
                    {
                        await ReplyAsync("Successfully removed tag.");
                    }
                    else await ReplyAsync("Tag not found.");
                }
            }
        }
    }
}