using BrowserHistoryGatherer.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Utilities
{
    internal class BrowserHistory
    {
        public static List<HistoryEntry> GetHistoryEntries()
        {
            List<HistoryEntry> historyEntries = new List<HistoryEntry>();

            var chromeEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(BrowserHistoryGatherer.Browser.Chrome, DateTime.Today);
            var fireFoxEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(BrowserHistoryGatherer.Browser.Firefox, DateTime.Today);
            var ieEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(BrowserHistoryGatherer.Browser.InternetExplorer, DateTime.Today);
            
            historyEntries.AddRange(chromeEntries);
            historyEntries.AddRange(fireFoxEntries);
            historyEntries.AddRange(ieEntries);
            
            return historyEntries;
        }
    }
}
