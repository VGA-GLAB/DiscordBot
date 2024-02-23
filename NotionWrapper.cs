using Notion;
using Notion.Client;
using System.Diagnostics;
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

    [Serializable]
    public class DiscordRole
    {
        public string RoleName;
        public string[] Tags;
        public string Emoji;
        public string RoleId;
        public string child_id;
        public string page_type;
        public string url;
        public string Sort;
    }

    public class Notice
    {
        public int Type;
        public int Chain;
    }

    public class NotionWrapper
    {
        SettingData _setting;
        NotionClient _client;

        public void Setup()
        {
            _setting = LocalData.Load<SettingData>("setting.json");
            _client = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = "secret_3AyEJHx6KjA1mEd6sOdQh5mOsNHmzxFWMVK2Cg6tW5E" //_setting.NotionAPIToken
            });
        }

        /// <summary>
        /// カレンダー取得用
        /// </summary>
        /// <returns></returns>
        public async void GetCalendar()
        {
            var dateFilter = new StatusFilter("通知ステータス", doesNotEqual: "完了");
            var queryParams = new DatabasesQueryParameters { Filter = dateFilter };
            var cancel = new CancellationToken();
            var pages = await _client.Databases.QueryAsync("42d144766a3d4a7d848768e5e9b37cac", queryParams, cancel);

            List<Notice> notices = new List<Notice>();

            if (pages == null) return;
            foreach(var p in pages.Results)
            {
                PropertyValue value;

                p.Properties.TryGetValue("通知ステータス", out value);
                StatusPropertyValue status = value as StatusPropertyValue;
                if(status.Status.Name == "予定")
                {
                    Notice n = new Notice();
                    n.Type = 1;
                }
            }
        }
        
        /// <summary>
        /// ロール取得用
        /// </summary>
        /// <returns></returns>
        public async void GetDiscordRole()
        {
            //var dateFilter = new StatusFilter("通知ステータス", doesNotEqual: "完了");
            //var queryParams = new DatabasesQueryParameters { Filter = dateFilter };
            //var pages = await _client.Databases.QueryAsync("fa4bdd28486348028c651b43c4e23feb");

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