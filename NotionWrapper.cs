using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network;
using System.Runtime.InteropServices;
using static Program;

namespace DiscordBot
{
    [Serializable]
    public class CalendarDate
    {
        public string start;
        public string end;
        public string time_zone;
    }

    [Serializable]
    public class NotionCalendar
    {
        public string[] Tags;
        public CalendarDate Date;
        public string[] Place;
        public string[] Member;
        public string Name;
        public string child_id;
        public string page_type;
        public string url;
    }

    public class NotionWrapper
    {
        /// <summary>
        /// カレンダー取得用
        /// </summary>
        /// <returns></returns>
        static public NotionCalendar[] GetCalendar()
        {
            string json = null;
            var settings = LocalData.Load<SettingData>("setting.json");

            int retryCount = 0;
            while (json == null)
            {
                json = WebRequest.GetRequest(string.Format("https://meodtz40k5.execute-api.ap-northeast-1.amazonaws.com/default/GetConfig/{0}/", settings.CalendarDBId)).GetAwaiter().GetResult();
                if (json != null) break;
                Task.Delay(1000).Wait();
                retryCount++;
                if(retryCount>3)
                {
                    Console.WriteLine("Error: server error.");
                    break;
                }
            }

            var objs = JsonConvert.DeserializeObject<NotionCalendar[]>(json);
            return objs;
        }
    }
}