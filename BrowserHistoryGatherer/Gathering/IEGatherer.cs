using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using BrowserHistoryGatherer.Data;
using UrlHistoryLibrary;

namespace BrowserHistoryGatherer.Gathering
{
    /// <summary>
    /// A gatherer to get ie history entries
    /// </summary>
    internal sealed class IEGatherer : BaseGatherer
    {
        #region Private Members
        private const string EDGE_DATA_PATH = @"Microsoft\Edge\User Data\Default\";
        private const string DATABASE_NAME = "History";
        #endregion


        #region Public Properties

        public static IEGatherer Instance { get; } = new IEGatherer();

        #endregion


        #region Events

        #endregion



        /// <summary>
        /// Initializes a new instance of <see cref="IEGatherer"/>
        /// </summary>
        IEGatherer()
        {

        }



        public sealed override ICollection<HistoryEntry> GetBrowserHistory(DateTime? startTime, DateTime? endTime)
        {
            List<HistoryEntry> entryList = new List<HistoryEntry>();

            var historyEnumertator = GetHistoryEnumerator();
            while (historyEnumertator.MoveNext())
            {
                Uri uri;
                DateTime lastUpdate;
                string title;

                lastUpdate = historyEnumertator.Current.LastUpdated;
                if (!base.IsEntryInTimelimit(lastUpdate, startTime, endTime))
                    continue;

                try
                {
                    uri = new Uri(historyEnumertator.Current.URL, UriKind.Absolute);
                }
                catch (UriFormatException)
                {
                    continue;
                }

                title = string.IsNullOrEmpty(historyEnumertator.Current.Title)
                    ? null
                    : historyEnumertator.Current.Title;

                var historyEntry = new HistoryEntry(uri, title, lastUpdate.ToUniversalTime(), null, Browser.InternetExplorer);
                entryList.Add(historyEntry);
            }

            //Edge Browser
            var edgeBrowserHistoryPath = GetEdgeBrowserDatabasePath();
            if(!string.IsNullOrEmpty(edgeBrowserHistoryPath))
            {
                try
                {
                    using (var connection = new SQLiteConnection($"Data Source={edgeBrowserHistoryPath};Version=3;"))
                    {
                        connection.Open();
                        string query = "SELECT url, title, visit_count, datetime(last_visit_time/1000000-11644473600, 'unixepoch') As last_visit_time FROM urls";
                        using (var command = new SQLiteCommand(query, connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Uri uri = new Uri(reader["url"].ToString(), UriKind.Absolute);
                                    string title = Convert.ToString(reader["title"]) ?? "";
                                    int? visitCount = Convert.ToInt32(reader["visit_count"]);
                                    DateTime lastVisit = DateTime.Parse(reader["last_visit_time"].ToString()).ToLocalTime();
                                    if (!base.IsEntryInTimelimit(lastVisit, startTime, endTime))
                                        continue;
                                    var historyEntry = new HistoryEntry(uri, title, lastVisit.ToUniversalTime(), visitCount, Browser.InternetExplorer);
                                    entryList.Add(historyEntry);
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }

            return entryList;
        }


        private UrlHistoryWrapperClass.STATURLEnumerator GetHistoryEnumerator()
        {
            var historyWrapper = new UrlHistoryWrapperClass();
            return historyWrapper.GetEnumerator();
        }

        private string GetEdgeBrowserDatabasePath()
        {
            var databasePaths = string.Empty;

            string dataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), EDGE_DATA_PATH);

            if (Directory.Exists(dataFolder))
            {
                string dbPath = Path.Combine(dataFolder, DATABASE_NAME);
                if (File.Exists(dbPath))
                {
                    string historyCopyPath = Path.Combine(Path.GetTempPath(), "EdgeHistoryCopy");
                    // Copy the Edge history database to a temporary location because while using Edge browser sql file is locked
                    File.Copy(dbPath, historyCopyPath, true);
                    databasePaths = historyCopyPath;
                }
            }

            return databasePaths;
        }
    }
}