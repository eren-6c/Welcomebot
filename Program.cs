using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

class Program
{
    private DiscordSocketClient _client;
    private static string TOKEN = Environment.GetEnvironmentVariable("DISCORD_TOKEN")!;
    private static ulong WELCOME_CHANNEL_ID = ulong.Parse(Environment.GetEnvironmentVariable("WELCOME_CHANNEL_ID")!);
    private const ulong OWNER_ID = 1249079341766283340;

    private static readonly List<string> WelcomeMessages = new()
    {
        "🚀 Hold tight, {0} just crash-landed into {1}!",
        "🔥 Watch out! {0} just joined the chaos in {1}!",
        "🎉 Big entrance! {0} is now part of {1}. Let's make some noise!",
        "💥 Boom! {0} just popped into {1}. Brace yourselves!",
        "🌟 A wild {0} appeared in {1}!"
    };

    private Dictionary<DateTime, int> _dailyJoins = new Dictionary<DateTime, int>();

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
            AlwaysDownloadUsers = true
        };

        _client = new DiscordSocketClient(config);

        _client.Log += LogAsync;
        _client.Ready += ClientReady;
        _client.UserJoined += HandleUserJoinAsync;

        await _client.LoginAsync(TokenType.Bot, TOKEN);
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private async Task ClientReady()
    {
        Console.WriteLine($"✅ Bot connected as {_client.CurrentUser.Username}");
        await SendTestWelcomeMessage();
    }

    private async Task SendTestWelcomeMessage()
    {
        try
        {
            var guild = _client.Guilds.FirstOrDefault();
            if (guild == null)
            {
                Console.WriteLine("❌ No guilds found");
                return;
            }

            var channel = guild.GetTextChannel(WELCOME_CHANNEL_ID);
            if (channel == null)
            {
                Console.WriteLine($"❌ Welcome channel not found (ID: {WELCOME_CHANNEL_ID})");
                return;
            }

            var embed = new EmbedBuilder()
                .WithColor(new Color(88, 101, 242))
                .WithAuthor($"{guild.Name} • System Test", guild.IconUrl)
                .WithTitle("**SERVER OWNER NOTIFICATION**")
                .WithDescription($@"
**━━━━━━━━━━━━━━━━━━━━**

<@{OWNER_ID}>, this is a **system test** of our welcome message.

**Verification:** <#1324968508630630431>  
**Tickets:** <#1287465924487807168>  
**Premium:** <#1287465924487807168>  

> Members: {guild.MemberCount}
> Status: Active

**━━━━━━━━━━━━━━━━━━━━**
                ")
                .WithThumbnailUrl(guild.IconUrl)
                 .WithImageUrl("https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExYnRxcHhvcXQ2aHdtY3A4dTkwbzVrOWRsN24wNzJ2ZmljcDI3dnh3NyZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/pZkeAoTeU50MymobZY/giphy.gif")
                .WithFooter("System Test • Eren Xiter")
                .Build();

            await channel.SendMessageAsync(embed: embed);
            Console.WriteLine("✅ Owner test message sent");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Test message failed: {ex.Message}");
        }
    }

    private async Task HandleUserJoinAsync(SocketGuildUser user)
    {
        try
        {
            // Send to welcome channel
            var channel = user.Guild.GetTextChannel(WELCOME_CHANNEL_ID);
            if (channel != null)
            {
                var embed = await BuildWelcomeEmbed(user, isTest: false);
                await channel.SendMessageAsync(embed: embed);
            }

            // Send personal DM
            await SendWelcomeDM(user);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Welcome error: {ex.Message}");
        }
    }

    private async Task<Embed> BuildWelcomeEmbed(SocketGuildUser user, bool isTest)
    {
        var today = DateTime.Today;
        _dailyJoins[today] = _dailyJoins.TryGetValue(today, out var count) ? count + 1 : 1;

        return new EmbedBuilder()
            .WithColor(new Color(101, 43, 186))
            .WithAuthor(user.Guild.Name, user.Guild.IconUrl)
            .WithTitle(isTest ? "Test Message" : $"**Welcome {user.Username}!**")
            .WithDescription(string.Join("\n",
                $"{user.Mention} {(isTest ? "system check" : $"joined {user.Guild.Name}")}",
                "",
                "• **Verify:** <#1324968508630630431>",
                "• **Support:** <#1287465924487807168>",
                "• **Premium:** <#1287465924487807168>",
                "",
                $"**Members:** {user.Guild.MemberCount}",
                $"**Today:** {_dailyJoins[today]} new",
                "",
                isTest ? "*System verification*" : $"*Joined at {DateTime.Now:HH:mm}*"
            ))
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
             .WithImageUrl("https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExYnRxcHhvcXQ2aHdtY3A4dTkwbzVrOWRsN24wNzJ2ZmljcDI3dnh3NyZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/pZkeAoTeU50MymobZY/giphy.gif")
            .WithFooter("Eren Xiter - Bot Made By Eren")
            .Build();
    }

    private async Task SendWelcomeDM(SocketGuildUser user)
    {
        try
        {
            var dmEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle($"Welcome to {user.Guild.Name}!")
                .WithDescription(
                    $"Hello {user.Mention}!\n\n" +
                    "**Quick Start:**\n" +
                    "1. Verify in <#1324968508630630431>\n" +
                    "2. Read #rules\n" +
                    "3. Explore our channels\n\n" +
                    "Need help? Open a ticket!")
                .WithThumbnailUrl(user.Guild.IconUrl)
                 .WithImageUrl("https://media2.giphy.com/media/v1.Y2lkPTc5MGI3NjExYnRxcHhvcXQ2aHdtY3A4dTkwbzVrOWRsN24wNzJ2ZmljcDI3dnh3NyZlcD12MV9pbnRlcm5hbF9naWZfYnlfaWQmY3Q9Zw/pZkeAoTeU50MymobZY/giphy.gif")
                  .WithFooter("Eren Xiter - Bot Made By Eren")
                .Build();

            await user.SendMessageAsync(embed: dmEmbed);
        }
        catch
        {
            Console.WriteLine($"⚠ Couldn't DM {user.Username}");
        }
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}