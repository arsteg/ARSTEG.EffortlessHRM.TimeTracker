﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTrackerX.Utilities
{
    public static class CheckInternetConnectivity
    {
        //Creating the extern function...
        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(
            out int Description,
            int ReservedValue
        );

        //Creating a function that uses the API function...
        public static bool IsConnectedToInternet()
        {
            int Desc;
            return InternetGetConnectedState(out Desc, 0);
        }
    }
}
