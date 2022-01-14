using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using Serilog.Core;

namespace SteelSeries.Arctis7.Gear
{
    internal class FormsApp
    {
        private readonly Logger _logger;
        private readonly System.Timers.Timer _updateTimer;
        private readonly Arctis7Reader _reader;
        private NotifyIcon _icon;
        private Icon[] _chargeIcons;
        private int _batteryLevel = -1;

        public bool Exit { get; set; }

        public FormsApp(Logger logger)
        {
            _logger = logger;
            _reader = new Arctis7Reader(logger);

            InitIcons();

            UpdateIcon();

            _updateTimer = new System.Timers.Timer(5000);
            _updateTimer.Elapsed += UpdateIcon;
            _updateTimer.AutoReset = true;
            _updateTimer.Enabled = true;
        }

        ~FormsApp()
        {
            _updateTimer.Stop();
            _updateTimer.Dispose();
            _icon.Visible = false;
            _icon.Dispose();
        }

        private void InitIcons()
        {
            _chargeIcons = new Icon[101];

            for (int i = 0; i <= 100; i++)
            {
                try
                {
                    var bitmap = (Bitmap)Image.FromFile("Assets\\Icons\\" + i + ".png");

                    using var f1 = new MemoryStream(File.ReadAllBytes("Assets\\Icons\\" + i + ".png"));
                    using var f2 = new MemoryStream();

                    PngIconConverter.Convert(f1, f2, 32, true);

                    f2.Position = 0;
                    _chargeIcons[i] = new Icon(f2);
                }
                catch (Exception)
                {
                    _chargeIcons[i] = _chargeIcons.LastOrDefault(i => i != null);
                }
            }

            _icon = new NotifyIcon();
            _icon.Text = "Arctis 7 Battery Reader";

            ContextMenuStrip trayMenu = new();
            ToolStripMenuItem exitItem = new();

            exitItem.Text = "Exit";
            exitItem.Click += new System.EventHandler(ExitProgram);
            trayMenu.Items.Add(exitItem);

            _icon.ContextMenuStrip = trayMenu;

            _icon.Icon = _chargeIcons[0];

            _icon.Visible = true;
        }

        private void UpdateIcon(Object source = null, ElapsedEventArgs e = null)
        {
            if (!_reader.TryReadBattery(out int batteryCharge))
            {
                _reader.TryInitHIDDev();
            }

            if (batteryCharge == 0 && _batteryLevel != 0)
            {
                ShowNotification(false, Convert.ToInt32(batteryCharge));
            }

            if (batteryCharge > 0 && _batteryLevel == 0)
            {
                ShowNotification(true, Convert.ToInt32(batteryCharge));
            }

            _batteryLevel = batteryCharge;
            _icon.Icon = _chargeIcons[batteryCharge];
            _icon.Text = $"{batteryCharge} %";
        }

        private void ShowNotification(bool connected, int level)
        {
            string statusLabel = connected ? "Connected" : "Disconnected";
            string levelLabel = connected ? $"{level}%" : null;

            _icon.BalloonTipText = string.Join(" ", new[] { statusLabel, levelLabel });
            _icon.ShowBalloonTip(3000);
        }

        private void ExitProgram(object sender, EventArgs e)
        {
            Application.Exit();
            Exit = true;
        }
    }
}