// ANDRII BALAKHTIN: Ultima modifica: 2025-04-13 in 1:21 AM

using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.Data.SqlClient;

namespace TelegramBot_Console.Classi
{

    internal class UpdateHandlerBot : IUpdateHandler
    {

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is { Text: { } text } message)
            {
                long chatId = message.Chat.Id;

                if (!text.StartsWith("/start"))
                {
                    using (SqlConnection connection = new SqlConnection(DatabaseBot._connectionString))
                    {
                        await connection.OpenAsync();

                        string query = "SELECT abilitato FROM [dbo].[Utenti] WHERE ChatID = @ChatId";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@ChatId", chatId.ToString());
                            object? result = await command.ExecuteScalarAsync();

                            if (result != null && result != DBNull.Value && !(bool)result)
                            {
                                await botClient.SendMessage(
                                    chatId: chatId,
                                    text: "❌ Bot fermato. Il tuo account è stato disabilitato. Usa /start per registrarti.",
                                    cancellationToken: cancellationToken
                                );
                                return;
                            }
                        }
                    }
                }
            }

            var handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(botClient, update.Message!, cancellationToken),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!, cancellationToken),
                _ => Task.CompletedTask
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"*Error* durante l'elaborazione dell'aggiornamento: {exception}");
            }
        }


        public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                Console.WriteLine($"*Warring* di polling: {exception}");
                return Task.CompletedTask;
            }

            public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
            {
                Console.WriteLine($"*Error* di gestione: {exception}, Source: {source}");
                return Task.CompletedTask;
            }

            private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
            {
                if (message.Text is not { } messageText) return;
                Console.WriteLine($"*Received* '{message.Text}' in {message.Chat}");

                var action = messageText.Split(' ')[0] switch
                {
                    "/start"             => SendStartCommand(botClient, message, cancellationToken),
                    "/stop"              => StopUser(botClient,         message, cancellationToken),
                    "/stato"             => SendStato(botClient,        message, cancellationToken),
                    "/accendi_cucina"    => AccendiLuce(botClient,      message, "Cucina", cancellationToken),
                    "/spegni_cucina"     => SpegniLuce(botClient,       message, "Cucina", cancellationToken),
                    "/accendi_sala"      => AccendiLuce(botClient,      message, "Sala", cancellationToken),
                    "/spegni_sala"       => SpegniLuce(botClient,       message, "Sala", cancellationToken),
                    "/accendi_bagno"     => AccendiLuce(botClient,      message, "Bagno", cancellationToken),
                    "/spegni_bagno"      => SpegniLuce(botClient,       message, "Bagno", cancellationToken),
                    "/accendi_camera"    => AccendiLuce(botClient,      message, "Camera", cancellationToken),
                    "/spegni_camera"     => SpegniLuce(botClient,       message, "Camera", cancellationToken),
                    "/attiva_allarme"    => AttivaAllarme(botClient,    message, cancellationToken),
                    "/disattiva_allarme" => DisattivaAllarme(botClient, message, cancellationToken),
                    "/help"              => SendHelp(botClient, message, cancellationToken),
                    _ => SendUnknownCommand(botClient, message, cancellationToken)
                };

                await action;

            }

        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);

            long chatId = callbackQuery.Message!.Chat.Id;
            string? callbackData = callbackQuery.Data;

            switch (callbackData)
            {
                case "accendi_cucina":
                    await AccendiLuce(botClient, callbackQuery.Message, "Cucina", cancellationToken);
                    break;
                case "spegni_cucina":
                    await SpegniLuce(botClient, callbackQuery.Message, "Cucina", cancellationToken);
                    break;
                case "accendi_sala":
                    await AccendiLuce(botClient, callbackQuery.Message, "Sala", cancellationToken);
                    break;
                case "spegni_sala":
                    await SpegniLuce(botClient, callbackQuery.Message, "Sala", cancellationToken);
                    break;
                case "accendi_bagno":
                    await AccendiLuce(botClient, callbackQuery.Message, "Bagno", cancellationToken);
                    break;
                case "spegni_bagno":
                    await SpegniLuce(botClient, callbackQuery.Message, "Bagno", cancellationToken);
                    break;
                case "accendi_camera":
                    await AccendiLuce(botClient, callbackQuery.Message, "Camera", cancellationToken);
                    break;
                case "spegni_camera":
                    await SpegniLuce(botClient, callbackQuery.Message, "Camera", cancellationToken);
                    break;
                case "attiva_allarme":
                    await AttivaAllarme(botClient, callbackQuery.Message, cancellationToken);
                    break;
                case "disattiva_allarme":
                    await DisattivaAllarme(botClient, callbackQuery.Message, cancellationToken);
                    break;
                default:
                    if (callbackQuery.Data != "📋 Open menu")
                    {
                        await botClient.SendMessage(chatId: chatId, text: $"❓ Comando callback sconosciuto: {callbackData}", cancellationToken: cancellationToken);
                    }
                    break;
            }

            await EditMessageWithUpdatedKeyboard(botClient, callbackQuery, cancellationToken);
        }

        private static async Task EditMessageWithUpdatedKeyboard(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            long chatId = callbackQuery.Message!.Chat.Id;
            int messageId = callbackQuery.Message!.MessageId;
            string? callbackData = callbackQuery.Data;

            string updatedText = "📖 Scegli un'opzione:";

            var luci = DatabaseBot.GetOrCreateUserLuci(chatId);

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(luci["Cucina"] ? "💡" : "🌑")} Cucina {(luci["Cucina"] ? "Accesa" : "Spenta")}",
                        luci["Cucina"] ? "spegni_cucina" : "accendi_cucina"),

                    InlineKeyboardButton.WithCallbackData(
                        $"{(luci["Sala"] ? "💡" : "🌑")} Sala {(luci["Sala"] ? "Accesa" : "Spenta")}",
                        luci["Sala"] ? "spegni_sala" : "accendi_sala")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(luci["Bagno"] ? "💡" : "🌑")} Bagno {(luci["Bagno"] ? "Accesa" : "Spenta")}",
                        luci["Bagno"] ? "spegni_bagno" : "accendi_bagno"),

                    InlineKeyboardButton.WithCallbackData(
                        $"{(luci["Camera"] ? "💡" : "🌑")} Camera {(luci["Camera"] ? "Accesa" : "Spenta")}",
                        luci["Camera"] ? "spegni_camera" : "accendi_camera")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"{(DatabaseBot.GetOrCreateUserLuci(chatId)["Allarme"] ? "🚨" : "💤")} Allarme {(DatabaseBot.GetOrCreateUserLuci(chatId)["Allarme"] ? "Attivo" : "Disattivo")}",
                        DatabaseBot.GetOrCreateUserLuci(chatId)["Allarme"] ? "disattiva_allarme" : "attiva_allarme")
                }
            });
            try
            {
                await botClient.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: updatedText,
                    replyMarkup: keyboard,
                    cancellationToken: cancellationToken
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"*Error* editing message: {ex.Message}");
                await botClient.SendMessage(chatId: chatId, text: $"✅ Ricevuto: {callbackData}", cancellationToken: cancellationToken);
            }
        }


        #region Bot Command Functions

        private static async Task SendStartCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;

            using (SqlConnection connection = new SqlConnection(DatabaseBot._connectionString))
            {
                await connection.OpenAsync();

                string selectUserQuery = "SELECT Id, abilitato FROM [dbo].[Utenti] WHERE ChatID = @ChatId";
                int? utenteId = null;
                bool? abilitato = null;

                using (SqlCommand selectUserCommand = new SqlCommand(selectUserQuery, connection))
                {
                    selectUserCommand.Parameters.AddWithValue("@ChatId", chatId.ToString());
                    using (var reader = await selectUserCommand.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            utenteId = reader.GetInt32(0);
                            abilitato = !reader.IsDBNull(1) && reader.GetBoolean(1);
                        }
                    }
                }

                if (!utenteId.HasValue)
                {
                    string insertUserQuery = "INSERT INTO [dbo].[Utenti] (UtenteID, ChatID, abilitato) VALUES (@UtenteIdTelegram, @ChatId, 1); SELECT SCOPE_IDENTITY();";
                    int newUtenteId;

                    using (SqlCommand insertUserCommand = new SqlCommand(insertUserQuery, connection))
                    {
                        insertUserCommand.Parameters.AddWithValue("@UtenteIdTelegram", message.From?.Id.ToString());
                        insertUserCommand.Parameters.AddWithValue("@ChatId", chatId.ToString());
                        newUtenteId = Convert.ToInt32(await insertUserCommand.ExecuteScalarAsync());
                    }

                    string insertDatiQuery = "INSERT INTO [dbo].[Dati] (UtenteID, LedCucina, LedSala, LedBagno, LedCamera, AllarmeAttivo) VALUES (@UtenteId, 0, 0, 0, 0, 0)";
                    using (SqlCommand insertDatiCommand = new SqlCommand(insertDatiQuery, connection))
                    {
                        insertDatiCommand.Parameters.AddWithValue("@UtenteId", newUtenteId);
                        await insertDatiCommand.ExecuteNonQueryAsync();
                    }

                    Console.WriteLine($"*New user added* with ChatId: {chatId} and UtenteId: {newUtenteId}");
                }
                else if (abilitato == false)
                {
                    string updateUserQuery = "UPDATE [dbo].[Utenti] SET abilitato = 1 WHERE Id = @UtenteId";
                    using (SqlCommand updateCommand = new SqlCommand(updateUserQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@UtenteId", utenteId.Value);
                        await updateCommand.ExecuteNonQueryAsync();
                    }

                    Console.WriteLine($"*User re-enabled* with ChatId: {chatId} and UtenteId: {utenteId.Value}");
                }

                await DatabaseBot.LoadUserLuciFromDatabase(chatId);
            }

            var keyboard = new InlineKeyboardMarkup(new[]
            {
            new[] { InlineKeyboardButton.WithCallbackData("📋 Open menu") },
            });
            await botClient.SendMessage(
                chatId: chatId,
                text: "👋 Benvenuto!",
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }

        private static async Task StopUser(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;

            if (DatabaseBot.UserCancellationTokenSources.TryGetValue(chatId, out var userCancellationTokenSource))
            {
                userCancellationTokenSource.Cancel();
                DatabaseBot.UserCancellationTokenSources.Remove(chatId);
            }

            using var connection = new SqlConnection(DatabaseBot._connectionString);
            await connection.OpenAsync(cancellationToken);

            const string disableUserQuery = "UPDATE [dbo].[Utenti] SET abilitato = 0 WHERE ChatID = @ChatId";

            using var command = new SqlCommand(disableUserQuery, connection);
            command.Parameters.AddWithValue("@ChatId", chatId.ToString());
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected > 0)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❎ Il bot è stato disattivato.",
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "⚠️ Nessun account trovato da disabilitare. Usa /start per registrarti.",
                    cancellationToken: cancellationToken
                );
            }
        }

        private static async Task SendStato(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;
            int? utenteId = null;

            using (SqlConnection connection = new SqlConnection(DatabaseBot._connectionString))
            {
                await connection.OpenAsync();
                string selectUserIdQuery = "SELECT Id FROM [dbo].[Utenti] WHERE ChatID = @ChatId";
                using (SqlCommand selectUserIdCommand = new SqlCommand(selectUserIdQuery, connection))
                {
                    selectUserIdCommand.Parameters.AddWithValue("@ChatId", chatId.ToString());
                    object? result = await selectUserIdCommand.ExecuteScalarAsync();
                    if (result != null && result != DBNull.Value)
                    {
                        utenteId = (int)result;
                    }
                }

                if (utenteId.HasValue)
                {
                    string selectStatoQuery = @"
                        SELECT LedCucina, LedSala, LedBagno, LedCamera, AllarmeAttivo
                        FROM [dbo].[Dati]
                        WHERE UtenteID = @UtenteId;";

                    using (SqlCommand selectStatoCommand = new SqlCommand(selectStatoQuery, connection))
                    {
                        selectStatoCommand.Parameters.AddWithValue("@UtenteId", utenteId.Value);
                        using (SqlDataReader reader = await selectStatoCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string statoLuci = "💡 𝐒𝐭𝐚𝐭𝐨 𝐋𝐮𝐜𝐢:\n\n";
                                statoLuci += $"| 🍴 𝐂𝐮𝐜𝐢𝐧𝐚 - {(reader["LedCucina"] != DBNull.Value && (bool)reader["LedCucina"] ? "✅" : "❌")}\n";
                                statoLuci += $"| 🛋️ 𝐒𝐚𝐥𝐚 - {(reader["LedSala"] != DBNull.Value && (bool)reader["LedSala"] ? "✅" : "❌")}\n";
                                statoLuci += $"| 🚿 𝐁𝐚𝐠𝐧𝐨 - {(reader["LedBagno"] != DBNull.Value && (bool)reader["LedBagno"] ? "✅" : "❌")}\n";
                                statoLuci += $"| 🛏️ 𝐂𝐚𝐦𝐞𝐫𝐚 - {(reader["LedCamera"] != DBNull.Value && (bool)reader["LedCamera"] ? "✅" : "❌")}\n";
                                string statoAllarme = $"🚨 𝐒𝐭𝐚𝐭𝐨 𝐀𝐥𝐥𝐚𝐫𝐦𝐞 - {(reader["AllarmeAttivo"] != DBNull.Value && (bool)reader["AllarmeAttivo"] ? "❗️" : "💤")}";

                                await botClient.SendMessage(chatId: message.Chat.Id, text: $"{statoLuci}\n{statoAllarme}", cancellationToken: cancellationToken);
                                return;
                            }
                        }
                    }
                }
                await botClient.SendMessage(chatId: message.Chat.Id, text: "❌ Stato non disponibile per questo utente.", cancellationToken: cancellationToken);
            }
        }

        private static async Task AccendiLuce(ITelegramBotClient botClient, Message message, string luce, CancellationToken cancellationToken)
        {
            await BackendBot.AggiornaStatoLuce(message.Chat.Id, luce, true);
            await DatabaseBot.UpdateDatabaseState(message.Chat.Id);
        }

        private static async Task SpegniLuce(ITelegramBotClient botClient, Message message, string luce, CancellationToken cancellationToken)
        {
            await BackendBot.AggiornaStatoLuce(message.Chat.Id, luce, false);
            await DatabaseBot.UpdateDatabaseState(message.Chat.Id);
        }

        private static async Task AttivaAllarme(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;
            await BackendBot.AggiornaStatoAllarme(chatId, true);
            await UpdateDatabaseAlarmState(chatId, true);
        }

        private static async Task DisattivaAllarme(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            long chatId = message.Chat.Id;
            await BackendBot.AggiornaStatoAllarme(chatId, false); 
            await UpdateDatabaseAlarmState(chatId, false);
        }

        private static async Task UpdateDatabaseAlarmState(long chatId, bool attivo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DatabaseBot._connectionString))
                {
                    await connection.OpenAsync();
                    string selectUserIdQuery = "SELECT Id FROM [dbo].[Utenti] WHERE ChatID = @ChatId";
                    int? utenteId = null;
                    using (SqlCommand selectUserIdCommand = new SqlCommand(selectUserIdQuery, connection))
                    {
                        selectUserIdCommand.Parameters.AddWithValue("@ChatId", chatId.ToString());
                        object? result = await selectUserIdCommand.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            utenteId = (int)result;
                        }
                    }

                    if (utenteId.HasValue)
                    {
                        string updateQuery = @"
                            UPDATE [dbo].[Dati]
                            SET AllarmeAttivo = @Allarme
                            WHERE UtenteID = @UtenteId;";

                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                        {
                            command.Parameters.AddWithValue("@UtenteId", utenteId.Value);
                            command.Parameters.AddWithValue("@Allarme", attivo);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"*Warning* UtenteId not found for ChatId: {chatId} when updating alarm state.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"*Error* updating alarm state in database: {ex.Message}");
            }
        }

        private static async Task SendHelp(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            string helpMessage = "/start - Avvia il bot e mostra le opzioni disponibili\n" +
                                 "/stop - Ferma il bot per questo utente\n" +
                                 "/stato - Mostra lo stato attuale delle luci e dell'allarme\n" +
                                 "/accendi_cucina - Accende la luce in cucina\n" +
                                 "/spegni_cucina - Spegne la luce in cucina\n" +
                                 "/accendi_sala - Accende la luce in sala\n" +
                                 "/spegni_sala - Spegne la luce in sala\n" +
                                 "/accendi_bagno - Accende la luce in bagno\n" +
                                 "/spegni_bagno - Spegne la luce in bagno\n" +
                                 "/accendi_camera - Accende la luce in camera\n" +
                                 "/spegni_camera - Spegne la luce in camera\n" +
                                 "/attiva_allarme - Attiva l'allarme\n" +
                                 "/disattiva_allarme - Disattiva l'allarme\n" +
                                 "/help - Mostra questo messaggio di aiuto";

            await botClient.SendMessage(chatId: message.Chat.Id, text: helpMessage, cancellationToken: cancellationToken);
        }

        private static async Task SendUnknownCommand(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            await botClient.SendMessage(chatId: message.Chat.Id, text: "❓ Comando sconosciuto.", cancellationToken: cancellationToken);
        }

        #endregion

    }
}
