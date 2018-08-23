using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DDDDiscord
{
    class calendarLoad
    {
        public static CalendarService Run()
        {
            UserCredential user;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                user = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets, Startup.Scopes, "user", CancellationToken.None, new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential File saved to: " + credPath);
            }
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = user,
                ApplicationName = Startup.ApplicationName,
            });
            return service;
        }
        public static CalendarService s = Run();
        public static CalendarList list = getCalendar();
        public static CalendarList getCalendar()
        {
            CalendarListResource.ListRequest request = s.CalendarList.List();
            request.MaxResults = 20;
            return request.Execute();
        }
    }
}
