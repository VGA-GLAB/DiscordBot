using Discord;
using Discord.Rest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using static Program;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

namespace DiscordBot
{
    public class RestAPIWrapper
    {
        private readonly DiscordRestClient _restCli;

        public RestAPIWrapper()
        {
            _restCli = new DiscordRestClient();
        }

        async Task Login()
        {
            if (_restCli.LoginState != LoginState.LoggedIn)
            {
                var settings = LocalData.Load<SettingData>("setting.json");
                await _restCli.LoginAsync(TokenType.Bot, settings.DiscordBotToken);
            }
        }

        public async Task PostMessage(ulong channel, string text)
        {
            await Login();
            var Channel = await _restCli.GetChannelAsync(channel) as RestTextChannel;
            var Result = await Channel.SendMessageAsync(text);
        }

        public async Task<Dictionary<string, RestRole>> GetRoles(ulong guild)
        {
            await Login();
            var Guild = await _restCli.GetGuildAsync(guild);
            Dictionary<string, RestRole> result = new Dictionary<string, RestRole>();
            var list = Guild.Roles.ToList();
            foreach(var role in list)
            {
                result.Add(role.Name, role);
            }
            return result;
        }
        public async Task AddRoles(ulong guildId, ulong userId, ulong roleId)
        {
            await Login();
            await _restCli.AddRoleAsync(guildId, userId, roleId);
        }
    }
}
