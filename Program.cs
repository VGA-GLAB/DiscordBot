using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    [Serializable]
    public class SettingData
    {
        public string DiscordBotToken = "";
        public ulong GuildId;
        public ulong ChannelId;
        public string NotionAPIToken = "";
        public string CalendarDBId = "";
    }

    static string TimeString(string timeStr)
    {
        DateTime end = DateTime.Parse(timeStr);
        string time = end.ToString("HH:mm");
        return time;
    }

    static void Main(string[] args)
    {
        var settings = LocalData.Load<SettingData>("setting.json");

        NotionWrapper wrapper = new NotionWrapper();
        wrapper.Setup();
        wrapper.GetCalendar();

        return;
        var discordAPI = new RestAPIWrapper();
        var roles = discordAPI.GetRoles(settings.GuildId).GetAwaiter().GetResult();

        DateTime now = DateTime.Now;
        DateTime weekStart = now.AddDays(-(int)now.DayOfWeek).AddMinutes(-now.Minute).AddHours(-now.Hour).AddSeconds(-now.Second); //週初めの0時
        DateTime weekEnd = weekStart.AddDays(7); //週おわりの0時
        DateTime today = now.AddMinutes(-now.Minute).AddHours(-now.Hour).AddSeconds(-now.Second); //今日の0時
        DateTime schoolStart = today.AddMinutes(30).AddHours(9); //今日の9時30分
        DateTime schoolEnd = today.AddMinutes(30).AddHours(20); //今日の20時30分
        string startTime = "";
        string endTime = "";
        
        /*
        //週の始まりのテキスト
        List<string> strings = new List<string>();
        foreach (var c in calendar)
        {
            DateTime start = DateTime.Parse(c.Date.start);
            string line = "作業日日程";

            line = start.ToString("MM/dd(ddd)");
            line += "　";
            line += c.Name;
            line += "\n";

            //考慮期間外
            if (weekStart > start || start > weekEnd)
            {
                continue;
            }

            if (schoolStart > start || start > schoolEnd)
            {
                line += "　終日";
            }
            else
            {
                startTime = TimeString(c.Date.start);
                if (c.Date.end == null)
                {
                    endTime = "未定";
                }
                else
                {
                    endTime = TimeString(c.Date.end);
                }

                line += startTime + "～" + endTime;
            }
            strings.Add(line);
        }
        */
        /*
        //次の日のテキスト
        DateTime nextday = today.AddDays(1);
        DateTime nextdayEnd = nextday.AddDays(1);
        schoolStart = nextday.AddMinutes(30).AddHours(9); //今日の9時30分
        schoolEnd = nextday.AddMinutes(30).AddHours(20); //今日の20時30分
        foreach (var c in calendar)
        {
            DateTime start = DateTime.Parse(c.Date.start);

            //考慮期間外
            if (nextday > start || start > nextdayEnd)
            {
                continue;
            }

            string line = "";
            if (roles.ContainsKey(settings.MentionRole))
            {
                line += roles[settings.MentionRole].Mention + "\n";
            }

            line += "**明日の作業日についてお知らせします**\n\n";

            line += "**" + start.ToString("MM/dd (ddd)") + "**　";
            line += c.Name;
            line += "\n\n";

            if (schoolStart > start || start > schoolEnd)
            {
                line += "◆開放時間:　終日";
            }
            else
            {
                startTime = TimeString(c.Date.start);
                if (c.Date.end == null)
                {
                    endTime = "未定";
                }
                else
                {
                    endTime = TimeString(c.Date.end);
                }

                line += "◆開放時間:　" + startTime + "～" + endTime + "\n";
            }

            //場所とか
            if(c.Place != null)
            {
                line += "◆場所:　" + string.Join(" / ",c.Place)+"\n";
            }

            //参加者とか
            if(c.Member != null)
            {
                if (c.Member.Any(m => m.IndexOf("講師") != -1))
                {
                    line += "◆参加講師:　" + string.Join(" / ", c.Member.Where(m => m.IndexOf("講師") != -1)) + "\n";
                }
                line += "◆参加者(順不同):　" + string.Join(" / ", c.Member.Where(m => m.IndexOf("講師") == -1)) + "\n";
            }

            line += "\n予定の詳細はこちらのリンクから確認ください:\n" + c.url + "\n";

            discordAPI.PostMessage(settings.ChannelId, line).Wait();
        }
        */
    }
}