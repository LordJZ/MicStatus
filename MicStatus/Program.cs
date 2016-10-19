using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CUE.NET;
using CUE.NET.Devices.Generic;
using CUE.NET.Devices.Keyboard;
using CUE.NET.Devices.Keyboard.Enums;
using NAudio.CoreAudioApi;

namespace MicStatus
{
    static class Program
    {
        class MessageFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref Message m)
            {
                // WM_CLOSE
                if (m.Msg == 16)
                {
                    Application.Exit();
                    return true;
                }

                return false;
            }
        }

        const CorsairKeyboardKeyId KeyboardKey = CorsairKeyboardKeyId.G17;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            InitializeKeyboard(true);
            InitializeIcon();
            InitializeAudioDevice();

            Muted = !Volume.Mute;
            SetMuted(Volume.Mute);

            Timer = new Timer();
            Timer.Interval = 20000;
            Timer.Enabled = true;
            Timer.Tick += Timer_Tick;

            Application.AddMessageFilter(new MessageFilter());
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
        static Timer Timer;
        static MenuItem ToggleMuteMenuItem;
        static AudioEndpointVolume Volume;
        static CorsairLed Led;
        static bool Muted;
        static int SuppressClick;

        static void Timer_Tick(object sender, EventArgs e)
        {
            SetMuted(Volume.Mute);
        }

        static void InitializeKeyboard(bool firstTime = false)
        {
            Led = null;

            try
            {
                if (firstTime)
                    CueSDK.Initialize();
                else
                    CueSDK.Reinitialize();

                Led = CueSDK.KeyboardSDK[KeyboardKey].Led;
            }
            catch
            {
                if (!firstTime)
                {
                    try
                    {
                        InitializeKeyboard(true);
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
        }

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

            SetMuted(Volume.Mute);
        }

        static void SetMuted(bool muted)
        {
            if (Muted != muted)
            {
                Icon.Icon = muted ? Properties.Resources.MicRed : Properties.Resources.MicWhite;
                ToggleMuteMenuItem.Text = muted ? "Unmute" : "Mute";
            }

            try
            {
                if (Led == null)
                    InitializeKeyboard(true);

                if (Led != null)
                {
                    Led.Color = muted ? Color.FromArgb(170, 0, 0) : Color.Blue;
                    CueSDK.KeyboardSDK.Update();
                }
            }
            catch
            {
                // ignore
            }

            Muted = muted;
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
