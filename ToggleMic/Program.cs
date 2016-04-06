using System;
using NAudio.CoreAudioApi;

namespace ToggleMic
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            MMDevice audio = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            if (audio == null)
                return;

            AudioEndpointVolume volume = audio.AudioEndpointVolume;
            volume.Mute = !volume.Mute;
        }
    }
}
