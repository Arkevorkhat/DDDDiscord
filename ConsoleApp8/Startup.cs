using DSharpPlus;
using System;
using System.Threading.Tasks;
using System.Xml;
using Google.Apis.Calendar.v3;
using DSharpPlus.Interactivity;
using Google.Apis.Calendar.v3.Data;
using DSharpPlus.CommandsNext;
using Google.Apis.Auth.OAuth2;
using System.IO;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;

namespace DDDDiscord
{
    class Startup
    {
        public static DiscordClient discord;
        public static Events events;
        public static CalendarService service;
        public static XmlDocument coreStorage = new XmlDocument();
        public static string[] Scopes = { CalendarService.Scope.Calendar };
        public static string ApplicationName = "DDDDiscord";
        public static InteractivityModule interactivity;
        public static CalendarListEntry calendarListEntry = null;
        public static CommandsNextModule commands;
        public static EventsResource.ListRequest GetRequest()
        {
            var request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            return request;
        }
        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Google Calendar API service.
            service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            EventsResource.ListRequest request = GetRequest();

            // List events.
            events = request.Execute();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            foreach (string s in args) Console.WriteLine(s);
            AsyncMain(args).ConfigureAwait(false).GetAwaiter().GetResult();

        }
        public static async Task AsyncMain(string[] args)
        {

            if (args.Length < 2)
            {
                Console.WriteLine("You must pass at minimum 2 arguments: -t \"Token\"");
                return;
            }
            if (args[0].Equals("-t"))
            {
                discord = new DiscordClient(new DiscordConfiguration { Token = args[1], TokenType = TokenType.Bot, UseInternalLogHandler = true, LogLevel = LogLevel.Debug });
                interactivity = discord.UseInteractivity(new InteractivityConfiguration());
                commands = discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = "::",
                });
                /*
                 * Code below this line must be left intact.
                 */
                commands.RegisterCommands<Commands>();
                await discord.ConnectAsync();
                await Task.Delay(-1);
            }
            else
            {
                Console.Write("Invalid Arugment");
            }

        }
    }
}
