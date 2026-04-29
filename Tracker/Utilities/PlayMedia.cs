using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Utilities
{
    public class PlayMedia
    {
        public static void PlayScreenCaptureSound()
        {
            using (var player = new System.Media.SoundPlayer(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "media", "audio", "screencaptureSound.wav")))
            {
                player.Play();
            }
        }
    }
}
