// ANDRII BALAKHTIN: Ultima modifica: 2025-04-13 in 11:21 PM

using Telegram.Bot;

namespace TelegramBot_Console.Classi
{
    internal class BackendBot
    {
        public static ITelegramBotClient TelegramBot { get; set; } = null!;

        #region Luce Functions

        public static async Task AggiornaStatoLuce(long chatId, string luce, bool stato)
        {
            await DatabaseBot.AggiornaStatoLuce(chatId, luce, stato);
        }

        #endregion

        #region Allarme Functions

        public static async Task AggiornaStatoAllarme(long chatId, bool stato)
        {

            var luci = DatabaseBot.GetOrCreateUserLuci(chatId);
            luci["Allarme"] = stato;

            Console.WriteLine($"*Backend* Allarme aggiornato a {stato} per utente {chatId}");

            await DatabaseBot.UpdateDatabaseState(chatId);
        }

        public static async Task NotificaAllarme(long chatId)
        {
            Console.WriteLine("*Backend* Intrusione rilevata!");
            await TelegramBot.SendMessage(chatId, "Intrusione rilevata in sala!");
        }

        #endregion
    }
}
