// ANDRII BALAKHTIN: Ultima modifica: 2025-04-07 in 16:32 PM

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

            var botClient = new TelegramBotClient("7907367040:AAGO0DMuJR4PKVSufBV1X0uJzPYa6YiLyGM");
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