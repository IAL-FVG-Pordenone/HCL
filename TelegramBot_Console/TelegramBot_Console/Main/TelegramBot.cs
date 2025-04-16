// ANDRII BALAKHTIN: Ultima modifica: 2025-04-16 in 11:01 PM

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using TelegramBot_Console.Classi;

namespace TelegramBot_Console.Main
{

    public class TelegramBot
    {

        public static async Task Main()
        {
            // Initialize the bot client with your token -> (replace with your actual token via link - https://web.telegram.org/k/#@BotFather)
            var botClient = new TelegramBotClient("123456789:AAABBBGGGDDDFFFGGGHHHJJJJKKKK");
            BackendBot.TelegramBot = botClient;

            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() };
            botClient.StartReceiving(
                updateHandler: new UpdateHandlerBot(),
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMe(cancellationToken: cts.Token);
            Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
            Console.ReadLine();

            cts.Cancel();

        }
    }
}