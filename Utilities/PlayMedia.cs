using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Utilities
{
    public class PlayMedia
    {
        public static void PlayScreenCaptureSound()
        {
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(@$"{Environment.CurrentDirectory}\media\audio\screencaptureSound.wav");
            player.Play();
        }
    }
}
