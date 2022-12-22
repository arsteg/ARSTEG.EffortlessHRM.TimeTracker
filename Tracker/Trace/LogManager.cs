using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Trace
{
    public class LogManager
    {
        public static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    }
}
