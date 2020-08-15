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
    public class TagModule : SiliconModule
    {
        public Services.TagService Tags { get; set; }

        //TODO: tag edit

        [Name("~get")]
        [Command]
        [Summary("Gets a tag.")]
        public Task GetTag(string name, SocketGuildUser user = null)
        {
            if (Tags.TryGetTag(name, user ?? Context.User, out Tag value))
            {
                var builder = new EmbedBuilder()
                    .WithDescription(value.Text)
                    .WithTitle(name);
                if (value.Color != -1) builder.WithColor(new Color((uint)value.Color));
                if (value.Claimed) builder.WithAuthor(Context.Client.GetUser(value.Owner));
                return ReplyAsync(builder.Build());
            }
            return ReplyAsync($"Tag not found for {user}.");
        }

        [Command("find")]
        [Alias("search")]
        [Summary("Searches for a tag containing the follow text.")]
        public Task FindTags([Remainder] string search)
        {
            if (Tags.TryFindTag(search, out List<Tag> list))
            {
                int count = list.Count.Clamp(0, 10);
                var bldr = new StringBuilder();
                for (int i = 0; i < count; i++)
                {
                    Tag tag = list[i];
                    bldr.AppendLine($"{i + 1}. {Context.Client.GetUser(tag.Owner).Mention}: {tag.Name} " +
                        $"`|tag {tag.Name}{(tag.Claimed ? "" : " (Unclaimed)")} {tag.Owner}`");
                }
                return ReplyAsync(new EmbedBuilder()
                    .WithTitle($"Showing {count}/{list.Count} tags")
                    .WithDescription(bldr.ToString()).Build());
            }
            else return ReplyAsync("No tags found.");
        }

        [Command("list")]
        [Summary("Displays one page of tags.")]
        public Task ListTags(int page = 1)
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
                    $"`|tag {tag.Name}{(tag.Claimed ? "" : " (Unclaimed)")} {tag.Owner}`");
            }
            return ReplyAsync(new EmbedBuilder()
                .WithTitle($"Page {page + 1}/{pageCount}")
                .WithDescription(bldr.ToString()).Build());
        }

        [Remarks("tag management")]
        [Name("management")]
#pragma warning disable CA1034 // Nested types should not be visible
        public class ManagementModule : SiliconModule
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

            [Command("claim")]
            [Summary("Claims a tag without an owner.")]
            public async Task Claim(SocketGuildUser prevOwner, string name)
            {
                if (Tag.TryClaim(prevOwner, name))
                {
                    await ReplyAsync("Successfully claimed tag.");
                }
                await ReplyAsync("Could not find unclaimed tag.");
            }

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

            [Remarks("tag management admin")]
            [Group("admin")]
            [RequireUserPermission(ChannelPermission.ManageMessages)]
            public class AdminModule : SiliconModule
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