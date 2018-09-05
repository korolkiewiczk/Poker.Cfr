using System;
using MySql.Data.MySqlClient;

namespace Poker.Datalayer
{
    public class DbReader : IDisposable
    {
        private readonly string _dbName;
        private readonly MySqlConnection _connection;

        public DbReader(string dbName)
        {
            _dbName = dbName;
            _connection = ConnectToDatabase();
        }

        public bool GetPossibleActions(int pos, int hand, string actionHistory, out int? nextPos, out string actions, out int section, out string prob,
            out int? pay)
        {
            nextPos = null;
            actions = null;
            section = -1;
            prob = null;
            pay = null;

            using (MySqlCommand cmd =
                new MySqlCommand(
                    $"SELECT * FROM {_dbName} WHERE player = {pos} AND hand = {hand} AND actions = '{actionHistory}'",
                    _connection)
            )
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        nextPos = DbIntVal(reader, "NextPlayer");
                        actions = reader["PossibleActions"]?.ToString();
                        section = DbIntVal(reader, "Section").Value;
                        prob = reader["cfr"]?.ToString();
                        pay = DbIntVal(reader, "pay");
                        return true;
                    }
                }
            }
            return false;
        }

        private static int? DbIntVal(MySqlDataReader reader, string field)
        {
            object val = reader[field];
            if (val is DBNull) return null;
            return Convert.ToInt32(reader[field]);
        }


        private MySqlConnection ConnectToDatabase()
        {
            var connectionString = System.Configuration.ConfigurationManager.
                ConnectionStrings["master"].ConnectionString;

            var mySqlConnection = new MySqlConnection(connectionString);
            mySqlConnection.Open();
            return mySqlConnection;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
