using BotClient.Bussines.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BotClient.Bussines.Services
{
    public class MySQLService : IMySQLService
    {
        private readonly ISettingsService settingsService;

        public MySQLService(ISettingsService SettingsService)
        {
            settingsService = SettingsService;
        }

        public async Task<List<List<string>>> Select(string Query)
        {
            List<List<string>> result = new List<List<string>>();
            try
            {
                var settings = settingsService.GetServerSettings();
                string connectionString = $"Database={settings.MySQLConnectionSettings.DataBase};" +
                                          $"Datasource={settings.MySQLConnectionSettings.Host};" +
                                          $"User={settings.MySQLConnectionSettings.User};" +
                                          $"Password={settings.MySQLConnectionSettings.Password};";
                MySqlConnection mySQLConnection = new MySqlConnection(connectionString);
                mySQLConnection.Open();
                MySqlCommand mySQLCommand = new MySqlCommand(Query, mySQLConnection);
                using (MySqlDataReader reader = mySQLCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        List<string> resultRow = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            if (reader[i] != null)
                                resultRow.Add(reader[i].ToString());
                            else
                                resultRow.Add(string.Empty);
                        }
                        result.Add(resultRow);
                    }
                    reader.Close();
                }
                mySQLConnection.Close();
            }
            catch
            {
            //report about error
            }
            return result;
        }

        public async Task<bool> ExecuteNonQuery(string Query)
        {
            try
            {
                var settings = settingsService.GetServerSettings();
                string connectionString = $"Database={settings.MySQLConnectionSettings.DataBase};" +
                                          $"Datasource={settings.MySQLConnectionSettings.Host};" +
                                          $"User={settings.MySQLConnectionSettings.User};" +
                                          $"Password={settings.MySQLConnectionSettings.Password};";
                MySqlConnection mySQLConnection = new MySqlConnection(connectionString);
                mySQLConnection.Open();
                MySqlCommand mySQLCommand = new MySqlCommand(Query, mySQLConnection);
                mySQLCommand.ExecuteNonQuery();
                mySQLConnection.Close();
                return true;
            }
            catch
            {
                //report about error
            }
            return false;
        }
    }
}
