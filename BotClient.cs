using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Program;

namespace DiscordBot
{
    /// <summary>
    /// テスト用の応答BOT
    /// </summary>
    public class BotClient
    {
        DiscordSocketClient _client;

        public BotClient()
        {
            var configuracoes = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };
            _client = new DiscordSocketClient(configuracoes);
            _client.Log += LogAsync;
            _client.Ready += onReady;
            _client.MessageReceived += onMessage;
            //Discord.GatewayIntents.All
        }

        public async Task Run()
        {
            var settings = LocalData.Load<SettingData>("setting.json");
            await _client.LoginAsync(TokenType.Bot, settings.BotToken);
            await _client.StartAsync();

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
            var roles = _client.GetGuild(settings.GuildId).Roles.ToList();
            roles.ForEach(r =>
            {
                Console.WriteLine(r.Name);
                Console.WriteLine(r.Id);
                Console.WriteLine(r.Mention);
            });
            return Task.CompletedTask;
        }

        private async Task onMessage(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
            {
                return;
            }

            Console.WriteLine(message.Content);
            
            if (message.Content == "こんにちは")
            {
                await message.Channel.SendMessageAsync("こんにちは、" + message.Author.Username + "さん！");
            }
        }
    }
}
