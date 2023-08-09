using System;

namespace BrowserHistoryGatherer.Data
{
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

        public string SafeVisitCount => visitCount == null 
            ? "N/A" 
            : visitCount.ToString();

        #endregion


        #region Events

        #endregion



        /// <summary>
        /// Initializes a new instance of <see cref="HistoryEntry"/>
        /// </summary>
        public HistoryEntry(Uri uri, string title, DateTime visitTime, int? visitCount, Browser browser)
        {
            this.uri = uri;
            this.title = title;
            this.lastVisitTime = visitTime;
            this.visitCount = visitCount;
            this.browser = browser;
        }
    }
}