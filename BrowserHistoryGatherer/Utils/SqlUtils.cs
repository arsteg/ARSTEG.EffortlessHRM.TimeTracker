
using System.Data;
using System.Data.SQLite;

namespace BrowserHistoryGatherer.Utils
{
    public static class SqlUtils
    {
        public static DataTable QueryDataTable(string dbPath, string query)
        {
            SQLiteDataAdapter sqlDataAdapter;
            DataTable dataTable = new DataTable();
            
            string connectionString = string.Format("Data Source={0}", dbPath);
            try
            {
                using (SQLiteConnection sqlConnection = new SQLiteConnection(connectionString))
                {
                    sqlConnection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, sqlConnection))
                    {
                        sqlDataAdapter = new SQLiteDataAdapter(query, sqlConnection);
                        using (SQLiteDataReader rdr = cmd.ExecuteReader())
                        {
                            sqlDataAdapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
            }
            return dataTable;
        }
    }
}