using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace ScreenScraper {
    class Database {

        private static Database instance = null;
        private SQLiteConnection conn = null;

        private Database() {
            if(conn == null) {
                OpenConnection();
            }
        }

        public static Database Instance {
            get {
                if (instance == null) {
                    instance = new Database();
                }
                return instance;
            }
        }

        private void OpenConnection() {
            if(conn == null) {                
                conn = new SQLiteConnection("Data Source=database.db;Version=3;New=True;Compress=True;");                              
            }
            if(conn.State != System.Data.ConnectionState.Open) {
                try {
                    conn.Open();
                } catch (Exception) {}
            }
            
        }
        private void CloseConnection() {
            conn.Close();
        }
        private void CreateTables() {
            OpenConnection();
            SQLiteCommand command;
            command = conn.CreateCommand();
            string createTableCmd = "CREATE TABLE IF NOT EXISTS Games(Title VARCHAR(512) PRIMARY KEY, Link VARCHAR(512), " +
                "LastUpdated VARCHAR(512), Tags VARCHAR(512))";
            command.CommandText = createTableCmd;
            command.ExecuteNonQuery();

            createTableCmd = "CREATE TABLE IF NOT EXISTS Ignored(Title VARCHAR(512), FOREIGN KEY (Title) REFERENCES Games(Title))";
            command.CommandText = createTableCmd;
            command.ExecuteNonQuery();

            CloseConnection();                       
        }
        private void DropTable(string name) {
            OpenConnection();
            SQLiteCommand command;
            string createTableCmd = "DROP TABLE Games;";
            command = conn.CreateCommand();
            command.CommandText = createTableCmd;
            command.ExecuteNonQuery();
            CloseConnection();
        }
        public void InsertGame(Game game) {
            CreateTables();
            if(GameExists(game.Title)) {
                DeleteGame(game.Title);
            }

            OpenConnection();
            SQLiteCommand command;
           
            string insertCmd = "INSERT INTO Games (Title, Link, LastUpdated, Tags) VALUES('"
                + game.Title + "','"
                + game.Link + "','"
                + game.LastUpdated + "','"
                + game.GetTags() + "');";
            command = conn.CreateCommand();
            command.CommandText = insertCmd;
            command.ExecuteNonQuery();
            CloseConnection();
        }
        public List<Game> ReadData(string query) {
            List<Game> games = new List<Game>();

            OpenConnection();
            SQLiteDataReader reader;
            SQLiteCommand command;
            command = conn.CreateCommand();
            command.CommandText = query;
            reader = command.ExecuteReader();
            while(reader.Read()) {
                string title = reader.GetString(0);
                string link = reader.GetString(1);
                string date = reader.GetString(2);
                string tags = reader.GetString(3);
                List<string> strSplit = tags.Split().ToList(); ;
                Game game = new Game(title, link, date, strSplit);
                games.Add(game);
            }
            CloseConnection();
            return games;
        }

        public List<Game> GetGames() {
            return ReadData("SELECT * FROM Games LEFT JOIN Ignored on Games.Title = Ignored.Title WHERE Ignored.Title IS NULL");            
        }
        public List<Game> GetIgnored() {
            return ReadData("SELECT * FROM Games JOIN Ignored WHERE Games.Title = Ignored.Title;");
        }
        public void RestoreIgnored(string name) {
            OpenConnection();
            SQLiteCommand command = conn.CreateCommand();

            command.CommandText = "DELETE FROM Ignored WHERE Title = '" + CheckForQuote(name) + "'";
            command.ExecuteNonQuery();
            CloseConnection();
        }

        private bool TableExists(string name) {
            OpenConnection();
            bool retVal = false;
            SQLiteCommand command = conn.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + CheckForQuote(name) + "'";
            SQLiteDataReader reader = command.ExecuteReader();
            while(reader.Read()) {
                int result = reader.StepCount;
                if(result > 0) {
                    retVal = true;
                }
            }
            CloseConnection();
            return retVal;
        }

        private bool GameExists(string name) {
            OpenConnection();
            bool retVal = false;
            SQLiteCommand command = conn.CreateCommand();
            command.CommandText = "SELECT * FROM Games WHERE Title = '" + CheckForQuote(name) + "'";
            SQLiteDataReader reader = command.ExecuteReader();
            while(reader.Read()) {
                if(reader.StepCount > 0) {
                    retVal = true;
                }
            }
            CloseConnection();
            return retVal;
        }

        public void DeleteGame(string name) {
            OpenConnection();
            SQLiteCommand command = conn.CreateCommand();
            command.CommandText = "DELETE FROM Games WHERE Title = '" + CheckForQuote(name) + "'";
            command.ExecuteNonQuery();
            CloseConnection();
        }

        public void IgnoreGame(string name) {
            OpenConnection();
            SQLiteCommand command = conn.CreateCommand();
            command.CommandText = "INSERT INTO Ignored (Title) VALUES ('" + CheckForQuote(name) + "');";
            command.ExecuteNonQuery();
            CloseConnection();
        }

        private string CheckForQuote(string name) {
            int quote = name.IndexOf("'");
            if (quote > -1) {
                name = name.Insert(quote, "'");
            }
            return name;
        }
    }
}
