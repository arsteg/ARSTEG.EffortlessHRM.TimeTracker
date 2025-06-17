using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TimeTrackerX.Utilities
{
    public class PlayMedia
    {
        public static void PlayScreenCaptureSound()
        {
            string audioPath = Path.Combine(Environment.CurrentDirectory, "media", "audio", "screencaptureSound.wav");

            Process.Start("afplay", $"\"{audioPath}\"");
        }
    }
}
