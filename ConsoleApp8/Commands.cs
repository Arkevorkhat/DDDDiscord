using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDDDiscord
{
    class Commands
    {
        public static List<DateTime> dateTimes = new List<DateTime>();
        public enum FreqTypes { DAILY, WEEKLY, MONTHLY, YEARLY };
        [Command("claim")]
        public async Task ClaimEvent(CommandContext ctx)
        {
            Console.WriteLine("claimevent");
            var sender = ctx.Message.Author;
            var dmchannel = await Startup.discord.CreateDmAsync(sender);
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#1111aa"),
                Title = "List of Claimable Events",
                Timestamp = DateTime.Now
            };
            var events = Startup.GetRequest().Execute().Items.ToArray();
            for (int i = 0; i < events.Length; i++)
            {
                embed.Description += $"{{{i}}} : {events[i].Summary} at {events[i].Start.DateTime.ToString()}\n";
            }
            await dmchannel.SendMessageAsync(" ", false, embed.Build());
            var response = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(2));
            try
            {
                var resp = Convert.ToInt32(response.Message.Content);
                var id = "primary";
                var evId = events[resp].Id;
                Event e = events[resp];
                e.Description += ctx.Message.Author.Username; Startup.service.Events.Update(e, id, evId).Execute();
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }
        }
        [Command("test")]
        public async Task Test(CommandContext ctx)
        {
            await ctx.RespondAsync("Test Message Please Ignore");
        }
        [Command("setcalendar")]
        public async Task SetCalendar(CommandContext ctx)
        {
            Console.WriteLine("setCalendar");
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#1111aa"),
                Title = "List of Calendars",
                Timestamp = DateTime.Now,
            };
            for (var i = 0; i < calendarLoad.list.Items.Count; i++)
            {
                embed.Description += $"{{{i}}} : " + calendarLoad.list.Items[i].Summary + '\n';
            }
            await ctx.Message.RespondAsync("List of available calendars:", false, embed.Build());
            var repl = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(1));
            try
            {
                int choice = Convert.ToInt32(repl.Message.Content);
                Startup.calendarListEntry = calendarLoad.list.Items[choice];
                Console.Write(Startup.calendarListEntry.Summary);
            }
            catch (Exception e1) { Console.Write(e1.Message); }
        }
        [Command("get")]
        public async Task getEvent(CommandContext ctx)
        {
            var request = Startup.GetRequest();
            request.MaxResults = 1;
            var upcoming = await request.ExecuteAsync();
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#1111aa"),
                Title = "Upcoming Events",
                Timestamp = DateTime.Now

            };
            var next = upcoming.Items[0];
            embed.Description = $"{next.Summary} : {next.Start.DateTimeRaw} \n User \"{next.Description}\" Has signed up to bring donuts.";
            var dmChannel = await Startup.discord.CreateDmAsync(ctx.Message.Author);
            await dmChannel.SendMessageAsync(" ", false, embed.Build());
            return;
        }
        [Command("add")]
        public async Task addEvent(CommandContext ctx)
        {
            Console.WriteLine("AddEvent");
            try
            {
                Event evt = new Event()
                {
                    AnyoneCanAddSelf = true,
                };
                var x = DateTimeOffset.Now.Offset.Hours.ToString();
                var sender = ctx.Message.Author;
                var dmChannel = await Startup.discord.CreateDmAsync(sender);
                await dmChannel.SendMessageAsync("What is the name of this event?");
                var sum = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));
                evt.Summary = sum.Message.Content;
                await dmChannel.SendMessageAsync("What Date is the start of this event? yyyy-mm-dd");
                var startDate = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));
                await dmChannel.SendMessageAsync("What time does this event start at? 24h hh:mm");
                var startTime = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));

                evt.Start = new EventDateTime()
                {
                    DateTime = DateTime.Parse($"{startDate.Message.Content}T{startTime.Message.Content}:00.0000000"),
                    TimeZone = "America/Los_Angeles"
                };
                await dmChannel.SendMessageAsync("What Date is the end of this event? yyyy-mm-dd");
                var endDate = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));
                await dmChannel.SendMessageAsync("What time does this event end at? 24h hh:mm");
                var endTime = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));
                evt.End = new EventDateTime()
                {
                    DateTime = DateTime.Parse($"{endDate.Message.Content}T{endTime.Message.Content}:00.0000000"),
                    TimeZone = "America/Los_Angeles"
                };
                await dmChannel.SendMessageAsync("Does this event repeat? true/false");
                var stor = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));
                var repeat = Convert.ToBoolean(stor.Message.Content);
                if (repeat)
                {
                    await dmChannel.SendMessageAsync("How often does it repeat?", false, new DiscordEmbedBuilder()
                    {
                        Description = "(0) : Daily \n (1) : Weekly \n (2) : Monthly \n (3) Yearly",
                    }.Build());
                    var stor1 = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));
                    var repeatfreq = Convert.ToByte(stor1.Message.Content);
                    String freq;
                    switch (repeatfreq)
                    {
                        case 0:
                            freq = "DAILY";
                            break;
                        case 1:
                            freq = "WEEKLY";
                            break;
                        case 2:
                            freq = "MONTHLY";
                            break;
                        case 3:
                            freq = "YEARLY";
                            break;
                        default:
                            freq = "YEARLY";
                            break;
                    }
                    await dmChannel.SendMessageAsync("How many times does it repeat?");
                    var f = await Startup.interactivity.WaitForMessageAsync(xm => xm.Author == ctx.Message.Author, TimeSpan.FromMinutes(5));
                    int repeats = Convert.ToInt32(f.Message.Content);
                    evt.Recurrence = new String[] { $"RRULE:FREQ={freq};COUNT={repeats}" };
                }
                EventsResource.InsertRequest request = calendarLoad.s.Events.Insert(evt, "primary");
                Event createdEvent = await request.ExecuteAsync();
                await dmChannel.SendMessageAsync($"Event Created: {createdEvent.HtmlLink}");
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }
    }
}
