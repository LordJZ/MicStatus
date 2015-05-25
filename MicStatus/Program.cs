using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;

namespace MicStatus
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            InitializeIcon();
            InitializeAudioDevice();
            Application.ApplicationExit += OnExit;
            Application.Run();
        }

        static void InitializeIcon()
        {
            Icon = new NotifyIcon();
            Icon.ContextMenu = new ContextMenu(new[]
            {
                new MenuItem("Quit", QuitClicked)
            });
            Icon.Visible = true;
        }

        static NotifyIcon Icon;
        static AudioEndpointVolume Volume;

        static void InitializeAudioDevice()
        {
            DisposeVolume();

            MMDevice audio = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Capture, Role.Communications);
            if (audio != null)
            {
                Volume = audio.AudioEndpointVolume;
                Volume.OnVolumeNotification += VolumeUpdated;
            }

            UpdateDueToVolume();
        }

        static void UpdateDueToVolume()
        {
            if (Volume == null)
            {
                Icon.Visible = false;
                Application.Exit();
                return;
            }

            Icon.Icon = Volume.Mute ? Properties.Resources.MicRed : Properties.Resources.MicWhite;
        }

        static void VolumeUpdated(AudioVolumeNotificationData data)
        {
            try
            {
                UpdateDueToVolume();
            }
            catch
            {
                InitializeAudioDevice();
            }
        }

        static void DisposeVolume()
        {
            if (Volume == null)
                return;

            Volume.OnVolumeNotification -= VolumeUpdated;
            Volume.Dispose();
            Volume = null;
        }

        static void QuitClicked(object o, EventArgs e)
        {
            Application.Exit();
        }

        static void OnExit(object sender, EventArgs e)
        {
            DisposeVolume();
        }
    }
}
