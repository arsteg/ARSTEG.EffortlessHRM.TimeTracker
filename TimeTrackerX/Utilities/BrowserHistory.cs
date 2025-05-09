using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//using BrowserHistoryGatherer.Data;

namespace TimeTrackerX.Utilities
{
    internal class BrowserHistory
    {
        public static List<HistoryEntry> GetHistoryEntries()
        {
            List<HistoryEntry> historyEntries = new List<HistoryEntry>();

            //var chromeEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
            //    BrowserHistoryGatherer.Browser.Chrome,
            //    DateTime.Today
            //);
            //var fireFoxEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
            //    BrowserHistoryGatherer.Browser.Firefox,
            //    DateTime.Today
            //);
            //var ieEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
            //    BrowserHistoryGatherer.Browser.InternetExplorer,
            //    DateTime.Today
            //);

            //historyEntries.AddRange(chromeEntries);
            //historyEntries.AddRange(fireFoxEntries);
            //historyEntries.AddRange(ieEntries);

            return historyEntries;
        }

        public static List<HistoryEntry> GetHistoryEntries(DateTime startDate, DateTime endDate)
        {
            List<HistoryEntry> historyEntries = new List<HistoryEntry>();

            //var chromeEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
            //    BrowserHistoryGatherer.Browser.Chrome,
            //    startDate,
            //    endDate
            //);
            //var fireFoxEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
            //    BrowserHistoryGatherer.Browser.Firefox,
            //    startDate,
            //    endDate
            //);
            //var ieEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
            //    BrowserHistoryGatherer.Browser.InternetExplorer,
            //    startDate,
            //    endDate
            //);
            //var safariEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
            //    BrowserHistoryGatherer.Browser.Safari,
            //    startDate,
            //    endDate
            //);

            //historyEntries.AddRange(chromeEntries);
            //historyEntries.AddRange(fireFoxEntries);
            //historyEntries.AddRange(ieEntries);
            //historyEntries.AddRange(safariEntries);

            return historyEntries;
        }
    }

    public class HistoryEntry
    {
        #region Private Members

        #endregion


        #region Properties

        public Uri uri { get; }
        public string title { get; }
        public DateTime lastVisitTime { get; }
        public int? visitCount { get; }
        public Browser browser { get; }

        public string SafeVisitCount => visitCount == null ? "N/A" : visitCount.ToString();

        #endregion


        #region Events

        #endregion



        /// <summary>
        /// Initializes a new instance of <see cref="HistoryEntry"/>
        /// </summary>
        public HistoryEntry(
            Uri uri,
            string title,
            DateTime visitTime,
            int? visitCount,
            Browser browser
        )
        {
            this.uri = uri;
            this.title = title;
            this.lastVisitTime = visitTime;
            this.visitCount = visitCount;
            this.browser = browser;
        }
    }
}
