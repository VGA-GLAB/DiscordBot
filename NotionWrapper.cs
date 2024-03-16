using Notion;
using Notion.Client;
using System.Diagnostics;
using static Program;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiscordBot
{
    public class Plan
    {
        public string Id;
        public string Status;
        public DateTime SortDate;
        public string Day;
        public string Team;
        public string Place;
        public string Desc;
        public string Title;
        public string Link;
    }

    public class Announce
    {
        public DateTime CalcDate;
        public string Day;
        public string Desc;
        public string Status;
        public List<Plan> Chain = new List<Plan>();
    }

    public class NotionWrapper
    {
        SettingData _setting;
        NotionClient _client;

        string TimeString(string timeStr)
        {
            DateTime end = DateTime.Parse(timeStr);
            string time = end.ToString("HH:mm");
            return time;
        }

        public void Setup()
        {
            _setting = LocalData.Load<SettingData>("setting.json");
            _client = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = _setting.NotionAPIToken
            });
        }

        /// <summary>
        /// カレンダー取得用
        /// </summary>
        /// <returns></returns>
        public async Task<List<Announce>> GetCalendar()
        {
            var dateFilter = new StatusFilter("通知ステータス", doesNotEqual: "完了");
            var queryParams = new DatabasesQueryParameters { Filter = dateFilter };
            var cancel = new CancellationToken();
            var pages = await _client.Databases.QueryAsync(_setting.CalendarDBId, queryParams, cancel);

            List<Announce> announce = new List<Announce>();
            List<Plan> plans = new List<Plan>();
            
            if (pages == null) return announce;
            foreach(var p in pages.Results)
            {
                Plan plan = new Plan();
                PropertyValue value;

                p.Properties.TryGetValue("通知ステータス", out value);
                StatusPropertyValue status = value as StatusPropertyValue;
                if (status.Status == null)
                    continue;

                plan.Status = status.Status.Name;

                p.Properties.TryGetValue("日付", out value);
                DatePropertyValue date = value as DatePropertyValue;
                if (date?.Date == null) continue;

                if (date.Date.Start != null && date.Date.End != null)
                {
                    plan.SortDate = date.Date.Start.Value;
                    plan.Day = string.Format("{0}～{1}", date?.Date.Start?.ToString("HH:mm"), date?.Date.End?.ToString("HH:mm"));
                }
                else
                {
                    plan.SortDate = date.Date.Start.Value;
                    plan.Day = "終日";
                }

                p.Properties.TryGetValue("メンバー", out value);
                MultiSelectPropertyValue member = value as MultiSelectPropertyValue;
                if (member?.MultiSelect.Count > 0)
                {
                    var members = member?.MultiSelect.Select(n => n.Name).ToList();
                    plan.Team = string.Join(" / ", members); //place?.MultiSelect[0].Name;
                    //plan.Team = member?.MultiSelect[0].Name;
                }
                else
                {
                    plan.Team = "全チーム";
                }

                p.Properties.TryGetValue("教室", out value);
                MultiSelectPropertyValue place = value as MultiSelectPropertyValue;
                if (place?.MultiSelect.Count > 0)
                {
                    var places = place?.MultiSelect.Select(n => n.Name).ToList();
                    plan.Place = string.Join(" / ", places); //place?.MultiSelect[0].Name;
                }
                else
                {
                    plan.Place = "未定";
                }

                p.Properties.TryGetValue("用途", out value);
                RichTextPropertyValue desc = value as RichTextPropertyValue;
                if (desc?.RichText.Count > 0)
                {
                    plan.Desc = desc?.RichText[0].PlainText;
                }

                p.Properties.TryGetValue("予定", out value);
                TitlePropertyValue title = value as TitlePropertyValue;
                if (title?.Title.Count > 0)
                {
                    plan.Title = title?.Title[0].PlainText;
                }

                plan.Id = p.Id;
                plan.Link = p.Url;

                plans.Add(plan);
            }

            plans.Sort((a, b) =>
            {
                if (a.SortDate < b.SortDate)
                    return -1;
                else if (a.SortDate < b.SortDate)
                    return 1;
                else
                    return 0;
            });

            Announce an = new Announce();
            an.CalcDate = plans[0].SortDate;
            an.Day = plans[0].SortDate.ToString("M");
            an.Desc = plans[0].Desc;
            an.Status = plans[0].Status;
            an.Chain.Add(plans[0]);
            for (int i=1; i< plans.Count; ++i)
            {
                if(an.CalcDate.Day != plans[i].SortDate.Day && an.Desc != plans[i].Desc)
                {
                    announce.Add(an);
                    an = new Announce();
                    an.CalcDate = plans[i].SortDate;
                    an.Day = plans[i].SortDate.ToString("M");
                    an.Desc = plans[i].Desc;
                    an.Status = plans[i].Status;
                }
                an.Chain.Add(plans[i]);
            }
            announce.Add(an);
            return announce;
        }
        
        /// <summary>
        /// ロール取得用
        /// </summary>
        /// <returns></returns>
        public async Task<int> UpdateStatus(Announce an)
        {
            foreach(var plan in an.Chain)
            {
                DatabasesUpdateParameters updateParam = new DatabasesUpdateParameters();
                StatusPropertyValue value = new StatusPropertyValue() { Status = new StatusPropertyValue.Data() { Name = "リマインド" } };
                Dictionary<string, PropertyValue> updateParams = new Dictionary<string, PropertyValue>();
                switch (plan.Status)
                {
                    case "予定":
                        value.Status.Name = "リマインド";
                        updateParams.Add("通知ステータス", value);
                        break;

                    case "リマインド":
                        value.Status.Name = "完了";
                        updateParams.Add("通知ステータス", value);
                        break;
                }
                await _client.Pages.UpdatePropertiesAsync(plan.Id, updateParams);
            }
            return 0;
        }

        /// <summary>
        /// ユーザデータ送信用
        /// </summary>
        /// <returns></returns>
        static public void SetUserData(string name, string memberId)
        {

        }
    }
}