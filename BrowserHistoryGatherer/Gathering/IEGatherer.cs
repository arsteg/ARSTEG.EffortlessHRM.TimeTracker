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
        IEGatherer() { }

        public sealed override ICollection<HistoryEntry> GetBrowserHistory(
            DateTime? startTime,
            DateTime? endTime
        )
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

                var historyEntry = new HistoryEntry(
                    uri,
                    title,
                    lastUpdate.ToUniversalTime(),
                    0,
                    Browser.InternetExplorer
                );
                entryList.Add(historyEntry);
            }

            //Edge Browser
            var edgeBrowserHistoryPaths = GetEdgeBrowserDatabasePath();
            if (edgeBrowserHistoryPaths != null)
            {
                try
                {
                    foreach (string edgeBrowserHistoryPath in edgeBrowserHistoryPaths)
                    {
                        using (
                            var connection = new SQLiteConnection(
                                $"Data Source={edgeBrowserHistoryPath};Version=3;"
                            )
                        )
                        {
                            connection.Open();
                            string query =
                                "SELECT url, title, visit_count, datetime(last_visit_time/1000000-11644473600, 'unixepoch') As last_visit_time FROM urls";
                            using (var command = new SQLiteCommand(query, connection))
                            {
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        int visitCount = 0;
                                        Uri uri = new Uri(
                                            reader["url"].ToString(),
                                            UriKind.Absolute
                                        );
                                        string title = Convert.ToString(reader["title"]) ?? "";
                                        int.TryParse(
                                            reader["visit_count"]?.ToString(),
                                            out visitCount
                                        );
                                        DateTime lastVisit = DateTime
                                            .Parse(reader["last_visit_time"].ToString())
                                            .ToLocalTime();
                                        if (!base.IsEntryInTimelimit(lastVisit, startTime, endTime))
                                            continue;
                                        var historyEntry = new HistoryEntry(
                                            uri,
                                            title,
                                            lastVisit.ToUniversalTime(),
                                            visitCount,
                                            Browser.InternetExplorer
                                        );
                                        entryList.Add(historyEntry);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { }
            }

            return entryList;
        }

        private UrlHistoryWrapperClass.STATURLEnumerator GetHistoryEnumerator()
        {
            var historyWrapper = new UrlHistoryWrapperClass();
            return historyWrapper.GetEnumerator();
        }

        private List<string> GetEdgeBrowserDatabasePath()
        {
            var tempEdgeHistories = new List<string>();

            try
            {
                string dataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Microsoft",
                    "Edge",
                    "User Data"
                );

                List<string> edgeHistories = Directory
                    .GetDirectories(dataFolder, "Profile *")
                    .ToList();
                int i = 0;

                edgeHistories.Add(Path.Combine(dataFolder, "Default"));
                //edgeHistories.Add(Path.Combine(dataFolder, "Guest Profile")); // History is not captured in case of Guest profile

                foreach (string edgeHistory in edgeHistories)
                {
                    if (Directory.Exists(edgeHistory))
                    {
                        string dbPath = Path.Combine(edgeHistory, DATABASE_NAME);
                        if (File.Exists(dbPath))
                        {
                            string historyCopyPath = Path.Combine(
                                Path.GetTempPath(),
                                $"EdgeHistoryCopy_{i}"
                            );
                            // Copy the Edge history database to a temporary location because while using Edge browser sql file is locked
                            File.Copy(dbPath, historyCopyPath, true);
                            tempEdgeHistories.Add(historyCopyPath);
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex) { }

            return tempEdgeHistories;
        }
    }
}
