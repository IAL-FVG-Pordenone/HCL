// ANDRII BALAKHTIN: Ultima modifica: 2025-04-13 in 11:45 PM

using System.Data.SqlClient;

namespace TelegramBot_Console.Classi
{
    internal class DatabaseBot
    {
        public static Dictionary<long, Dictionary<string, bool>> UserLuci = new Dictionary<long, Dictionary<string, bool>>();
        public static Dictionary<long, CancellationTokenSource> UserCancellationTokenSources = new Dictionary<long, CancellationTokenSource>();

        public static readonly string _connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Users\\sndre\\Downloads\\AICA\\TelegramBot_Console\\DataBase\\Domotica.mdf;Integrated Security=True;Connect Timeout=30";

        public static Dictionary<string, bool> GetOrCreateUserLuci(long chatId)
        {
            if (!UserLuci.ContainsKey(chatId))
            {
                UserLuci[chatId] = new Dictionary<string, bool>
                {
                    { "Cucina",  false },
                    { "Sala",    false },
                    { "Bagno",   false },
                    { "Camera",  false },
                    { "Allarme", false }
                };
            }

            return UserLuci[chatId];
        }

        public static async Task UpdateDatabaseState(long chatId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectUserIdQuery = "SELECT Id FROM [dbo].[Utenti] WHERE ChatID = @ChatId";
                    int? utenteId = null;

                    using (SqlCommand selectUserIdCommand = new SqlCommand(selectUserIdQuery, connection))
                    {
                        selectUserIdCommand.Parameters.AddWithValue("@ChatId", chatId.ToString());
                        object result = await selectUserIdCommand.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                        {
                            utenteId = (int)result;
                        }
                    }

                    if (utenteId.HasValue)
                    {
                        string checkIfExistsQuery = "SELECT COUNT(*) FROM [dbo].[Dati] WHERE UtenteID = @UtenteId";
                        int recordCount = 0;

                        using (SqlCommand checkIfExistsCommand = new SqlCommand(checkIfExistsQuery, connection))
                        {
                            checkIfExistsCommand.Parameters.AddWithValue("@UtenteId", utenteId.Value);
                            recordCount = (int)await checkIfExistsCommand.ExecuteScalarAsync();
                        }

                        string query;
                        if (recordCount > 0)
                        {
                            query = @"
                                UPDATE [dbo].[Dati]
                                SET LedCucina = @Cucina,
                                    LedSala = @Sala,
                                    LedBagno = @Bagno,
                                    LedCamera = @Camera,
                                    AllarmeAttivo = @Allarme
                                WHERE UtenteID = @UtenteId;";
                        }
                        else
                        {
                            query = @"
                                INSERT INTO [dbo].[Dati] (UtenteID, LedCucina, LedSala, LedBagno, LedCamera, AllarmeAttivo)
                                VALUES (@UtenteId, @Cucina, @Sala, @Bagno, @Camera, @Allarme);";
                        }

                        var luci = GetOrCreateUserLuci(chatId);

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@UtenteId", utenteId.Value);
                            command.Parameters.AddWithValue("@Cucina",   luci["Cucina"]);
                            command.Parameters.AddWithValue("@Sala",     luci["Sala"]);
                            command.Parameters.AddWithValue("@Bagno",    luci["Bagno"]);
                            command.Parameters.AddWithValue("@Camera",   luci["Camera"]);
                            command.Parameters.AddWithValue("@Allarme",  luci["Allarme"]);

                            int rowsAffected = await command.ExecuteNonQueryAsync();
                        }
                    }
                    else
                    {
                        System.Console.WriteLine($"*Warning*: UtenteId not found for ChatId: {chatId}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"*Error* updating database: {ex.Message}");
            }
        }

        public static async Task LoadUserLuciFromDatabase(long chatId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string selectQuery = @"
                        SELECT D.LedCucina, D.LedSala, D.LedBagno, D.LedCamera, D.AllarmeAttivo
                        FROM [dbo].[Dati] D
                        JOIN [dbo].[Utenti] U ON D.UtenteID = U.Id
                        WHERE U.ChatID = @ChatId";

                    using (SqlCommand cmd = new SqlCommand(selectQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@ChatId", chatId.ToString());

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                UserLuci[chatId] = new Dictionary<string, bool>
                                {
                                    { "Cucina",  reader.GetBoolean(0) },
                                    { "Sala",    reader.GetBoolean(1) },
                                    { "Bagno",   reader.GetBoolean(2) },
                                    { "Camera",  reader.GetBoolean(3) },
                                    { "Allarme", reader.GetBoolean(4) }
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"*Error* loading user Luci: {ex.Message}");
            }
        }

        public static async Task AggiornaStatoLuce(long chatId, string luce, bool stato)
        {
            var luci = GetOrCreateUserLuci(chatId);

            if (luci.ContainsKey(luce))
            {
                luci[luce] = stato;
                System.Console.WriteLine($"*Action* Backend: Luce {luce} aggiornata a {stato} per utente {chatId}");
            }
            else
            {
                System.Console.WriteLine($"*Warring* Backend: Luce {luce} non trovata per utente {chatId}");
            }

            await UpdateDatabaseState(chatId);
        }

        public static async Task AggiornaStatoAllarme(long chatId, bool stato)
        {
            var luci = GetOrCreateUserLuci(chatId);
            luci["Allarme"] = stato;

            System.Console.WriteLine($"*Backend* Allarme aggiornato a {stato} per utente {chatId}");

            await UpdateDatabaseState(chatId);
        }
    }
}
