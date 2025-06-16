using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrowserHistoryGatherer;
using BrowserHistoryGatherer.Data;

namespace TimeTrackerX.Utilities
{
    internal class BrowserHistory
    {
        public static List<HistoryEntry> GetHistoryEntries()
        {
            List<HistoryEntry> historyEntries = new List<HistoryEntry>();
            
            var chromeEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
                BrowserHistoryGatherer.Browser.Chrome,
                DateTime.Today
            );
            var fireFoxEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
                BrowserHistoryGatherer.Browser.Firefox,
                DateTime.Today
            );
            var ieEntries = BrowserHistoryGatherer.BrowserHistory.GetHistory(
                BrowserHistoryGatherer.Browser.InternetExplorer,
                DateTime.Today
            );

            historyEntries.AddRange(chromeEntries);
            historyEntries.AddRange(fireFoxEntries);
            historyEntries.AddRange(ieEntries);

            return historyEntries;
        }

        public static List<HistoryEntry> GetHistoryEntries(DateTime startDate, DateTime endDate)
        {
            List<HistoryEntry> historyEntries = new List<HistoryEntry>();

            var safariEntries = GetSafariHistory(startDate,endDate);
            var chromeEntries = GetChromeHistory(startDate,endDate);
            var fireFoxEntries = GetFirefoxHistory(startDate,endDate);
            var ieEntries = GetIEHistory(startDate,endDate);
            

            historyEntries.AddRange(chromeEntries);
            historyEntries.AddRange(fireFoxEntries);
            historyEntries.AddRange(ieEntries);
            historyEntries.AddRange(safariEntries);

            return historyEntries;
        }

        public static IList<HistoryEntry> GetChromeHistory(DateTime startDate,DateTime endDate){
            try{
                return BrowserHistoryGatherer.BrowserHistory.GetHistory(
                BrowserHistoryGatherer.Browser.Chrome,
                startDate,
                endDate
            );
            }
            catch(Exception){
                return new List<HistoryEntry>();
            }
        }

        public static IList<HistoryEntry> GetFirefoxHistory(DateTime startDate,DateTime endDate){
            try{
                return BrowserHistoryGatherer.BrowserHistory.GetHistory(
                BrowserHistoryGatherer.Browser.Firefox,
                startDate,
                endDate
            );
            }
            catch(Exception){
                return new List<HistoryEntry>();
            }
        }

        public static IList<HistoryEntry> GetSafariHistory(DateTime startDate,DateTime endDate){
            try{
                return BrowserHistoryGatherer.BrowserHistory.GetHistory(
                BrowserHistoryGatherer.Browser.Safari,
                startDate,
                endDate
            );
            }
            catch(Exception){
                return new List<HistoryEntry>();
            }
        }

        public static IList<HistoryEntry> GetIEHistory(DateTime startDate,DateTime endDate){
            try{
                return BrowserHistoryGatherer.BrowserHistory.GetHistory(
                BrowserHistoryGatherer.Browser.InternetExplorer,
                startDate,
                endDate
            );
            }
            catch(Exception){
                return new List<HistoryEntry>();
            }
        }
    }
}
