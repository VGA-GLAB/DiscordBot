using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Program;
using static System.Net.WebRequestMethods;

namespace DiscordBot
{
    /// <summary>
    /// ロール付与用の応答BOT
    /// </summary>
    public class RoleAssignBotClient
    {
        DiscordSocketClient _client;
        DiscordRole[] _roles;
        SocketGuild _guild;

        Dictionary<string, SocketGuildUser> _userStack = new Dictionary<string, SocketGuildUser>();

        public RoleAssignBotClient()
        {
            var configuracoes = new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All
            };
            _client = new DiscordSocketClient(configuracoes);
            _client.Log += LogAsync;
            _client.Ready += onReady;
            _client.UserJoined += onJoinUser;
            _client.MessageReceived += onMessage;
            _client.ReactionAdded += onReaction;
            //Discord.GatewayIntents.All
        }

        public async Task Run()
        {
            var settings = LocalData.Load<SettingData>("setting.json");
            await _client.LoginAsync(TokenType.Bot, settings.BotToken);
            await _client.StartAsync();

            //確認用
            //var discordAPI = new RestAPIWrapper();
            //var roles = discordAPI.GetRoles(settings.GuildId).GetAwaiter().GetResult();

            _roles = NotionWrapper.GetDiscordRole();
            Array.Sort(_roles, (a, b) => int.Parse(a.Sort) - int.Parse(b.Sort));

            await Task.Delay(Timeout.Infinite);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task onReady()
        {
            Console.WriteLine($"{_client.CurrentUser} is Running!!");
            var settings = LocalData.Load<SettingData>("setting.json");
            _guild = _client.GetGuild(settings.GuildId);
            var roles = _guild.Roles.ToList();
            roles.ForEach(r =>
            {
                Console.WriteLine(r.Name);
                Console.WriteLine(r.Id);
                Console.WriteLine(r.Mention);
            });
            return Task.CompletedTask;
        }

        private string GetTeamMessage(string name)
        {
            string jobMessage = "こんにちは、" + name + "さん！ \r\n**あなたの「所属チーム」を教えてください！**\r\n\r\n";
            jobMessage += "----\r\nチームと一致するリアクションを押してください！\n\n";
            foreach (var r in _roles)
            {
                if (!r.RoleName.Contains("チーム")) continue;
                jobMessage += Emoji.Parse(r.Emoji) + " " + r.RoleName + "\r\n";
            }
            return jobMessage;
        }

        private string GetRoleMessage(string name)
        {
            string jobMessage = "こんにちは、" + name + "さん！ \r\n**あなたの「担当職種」を教えてください！**\r\n\r\nどの職種でアサインされているかは、以下の資料を確認してください。\r\nhttps://candle-stoplight-544.notion.site/5294189c9dc44dbaa6114629aa007fba?pvs=4\r\n\r\nなお、メインジョブ・サブジョブ共に「これもやりたい」ロールを追加してもかまいません。\r\n\r\n";
            jobMessage += "----\r\nメインジョブ\r\n\r\n以下の担当職種と一致するリアクションを押してください！\n\n";
            foreach (var r in _roles)
            {
                if (!r.RoleName.Contains("メインジョブ")) continue;
                jobMessage += Emoji.Parse(r.Emoji) + " " + r.RoleName.Replace("メインジョブ:","")+ "\r\n";
            }
            jobMessage += "----\r\n\r\n----\r\nサブジョブ\r\n\r\n";
            foreach (var r in _roles)
            {
                if (!r.RoleName.Contains("サブジョブ")) continue;
                jobMessage += Emoji.Parse(r.Emoji) + " " + r.RoleName.Replace("サブジョブ:", "") + "\r\n";
            }
            jobMessage += "----\r\n\r\n※サブジョブは人数が少なく需要が変動する可能性があるため、チームの垣根を越えてやり取りする可能性があります。\r\n";
            return jobMessage;
        }

        private async Task SendTeamMessage(SocketUser user)
        {
            //ロールを設定してもらう旨をDMする
            var message = await user.SendMessageAsync(GetTeamMessage(user.Username));
            foreach (var r in _roles)
            {
                if (!r.RoleName.Contains("チーム")) continue;
                await message.AddReactionAsync(Emoji.Parse(r.Emoji));
            }
        }

        private async Task SendRoleMessage(SocketUser user)
        {
            //ロールを設定してもらう旨をDMする
            var message = await user.SendMessageAsync(GetRoleMessage(user.Username));
            foreach (var r in _roles)
            {
                if (!r.RoleName.Contains("メインジョブ") && !r.RoleName.Contains("サブジョブ")) continue;
                await message.AddReactionAsync(Emoji.Parse(r.Emoji));
            }
        }

        private async Task onJoinUser(SocketGuildUser user)
        {
            //対象チャンネル以外では反応しない
            if (user.Guild.Id != 1136199339417534606)
            {
                return;
            }

            Console.WriteLine(user.Guild.Name);
            Console.WriteLine(user.Username);

            //チーム
            await SendTeamMessage(user);

            //担当職種
            await SendRoleMessage(user);
        }

        private async Task onMessage(SocketMessage message)
        {
            //専用チャンネル以外では反応しない
            /*
            if(message.Channel.Id != 1157848955946811553)
            {
                return;
            }
            */

            //BOT君の発言は読まない
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            Console.WriteLine(message.Content);

            if (message.Content == "ジョブ")
            {
                //ロールを設定してもらう旨をDMする
                await SendRoleMessage(message.Author);
            }

            if (message.Content == "チーム")
            {
                //チームを設定してもらう旨をDMする
                await SendTeamMessage(message.Author);
            }
        }


        private async Task onReaction(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            //BOT君の発言は読まない
            if (reaction.UserId == _client.CurrentUser.Id)
            {
                return;
            }

            Console.WriteLine(message.Value);
            Console.WriteLine(reaction.Emote.Name);

            //BOT君の発言は読まない
            foreach (var r in _roles)
            {
                if (reaction.Emote.Name == r.Emoji)
                {
                    var user = _guild.Users.Where(u => u.Id == reaction.UserId);
                    if (user.Count() == 0) break;
                    await user.First().AddRoleAsync(ulong.Parse(r.RoleId));
                }
            }
        }
        
    }
}
