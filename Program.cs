using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot;
using Notion.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        public string DiscordSettingDBId = "";
    }

    static void Main(string[] args)
    {
        var settings = LocalData.Load<SettingData>("setting.json");

        var discordAPI = new RestAPIWrapper();
        var roles = discordAPI.GetRoles(settings.GuildId).GetAwaiter().GetResult();

        NotionWrapper wrapper = new NotionWrapper();
        wrapper.Setup();
        var list = wrapper.GetCalendar().GetAwaiter().GetResult();

        var everyone = roles.Where(r => r.Value.Name == "@everyone").FirstOrDefault().Value;
        
        //アナウンスデータをもとにアナウンスする
        foreach (var announce in list)
        {
            if (announce.Status == "予定")
            {
                if( (announce.CalcDate - DateTime.Now).TotalDays > 36)
                {
                    continue;
                }
            }
            else
            {
                if ((announce.CalcDate - DateTime.Now).TotalDays > 1)
                {
                    continue;
                }
            }

            string text = "";
            text = string.Format("<@&{0}> \n", everyone.Id);
            if (announce.Status == "予定")
            {
                text += "G-Lab活動予定日のアナウンスです。\n";
            }
            else
            {
                text += "G-Lab活動日のリマインドです。\n";
            }

            text += "\n------------------------------------------\n";
            text += string.Format("■{0} ({1}) \n", announce.Day, announce.CalcDate.ToString("dddd"));
            foreach (var plan in announce.Chain)
            {
                text += string.Format("**{0}: {1}** 教室:{2} \n", plan.Day, plan.Team, plan.Place);
            }

            text += "\n◆概要\n";
            text += announce.Desc + "\n";
            text += "\n------------------------------------------\n\n";

            text += "詳細はそれぞれカレンダーリンクから確認ください\n";
            foreach (var plan in announce.Chain)
            {
                text += string.Format("{0}: {1} \n", plan.Team, plan.Link);
            }

            discordAPI.PostMessage(settings.ChannelId, text).Wait();

            wrapper.UpdateStatus(announce).GetAwaiter().GetResult();
        }
    }
}