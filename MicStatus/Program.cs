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
            SetMuted(Volume.Mute);
            Application.ApplicationExit += OnExit;
            Application.Run();
        }

        static void InitializeIcon()
        {
            Icon = new NotifyIcon();
            Icon.Click += ToggleMuteClicked;
            Icon.ContextMenu = new ContextMenu(new[]
            {
                ToggleMuteMenuItem = new MenuItem("Mute", ToggleMuteClicked),
                new MenuItem("Quit", QuitClicked)
            });
            Icon.ContextMenu.Popup += ContextMenu_Popup;
            Icon.Visible = true;
        }

        static NotifyIcon Icon;
        static MenuItem ToggleMuteMenuItem;
        static AudioEndpointVolume Volume;
        static bool Muted;
        static int SuppressClick;

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
                Application.Exit();
                return;
            }

            bool muted = Volume.Mute;
            if (muted == Muted)
                return;

            SetMuted(muted);
        }

        static void SetMuted(bool muted)
        {
            Muted = muted;
            if (muted)
            {
                Icon.Icon = Properties.Resources.MicRed;
                ToggleMuteMenuItem.Text = "Unmute";
            }
            else
            {
                Icon.Icon = Properties.Resources.MicWhite;
                ToggleMuteMenuItem.Text = "Mute";
            }
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

        static void ContextMenu_Popup(object sender, EventArgs e)
        {
            ++SuppressClick;
        }

        static void ToggleMuteClicked(object o, EventArgs e)
        {
            if (SuppressClick > 0)
            {
                --SuppressClick;
                return;
            }

            Volume.Mute = !Volume.Mute;
        }

        static void QuitClicked(object o, EventArgs e)
        {
            Application.Exit();
        }

        static void OnExit(object sender, EventArgs e)
        {
            Icon.Dispose();
            DisposeVolume();
        }
    }
}
