using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Tonjiru
{
    using System.Runtime.InteropServices;
    using System.Windows.Forms; // 初期設定では参照されていないので追加しておく

    // 修飾しないと System.Windows.Forms.Application に解釈されてコンパイルできない
    public partial class App : System.Windows.Application
    {
        [System.STAThreadAttribute()]
        public static void Main()
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Contains("/g") || (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                var app = new Tonjiru.App();
                app.InitializeComponent();
                app.Run();
            }
            else // UI less mode
            {
                CloseAllWindowsAndExit();
            }
        }

        private static void CloseAllWindowsAndExit()
        {
            try
            {
                var exclusions = System.IO.File.ReadAllLines("exclusions.txt");

                foreach (var window in DesktopHelper.GetVisibleWindows())
                {
                    if (exclusions.Any(_ => _ == window.Parent.ProcessName.ToLower()))
                    {
                        continue;
                    }

                    window.Close();
                }

                if (Tonjiru.Properties.Settings.Default.Notification)
                {
                    NotificationHelper.ShowBalloonTip();
                }
            }
            catch (Exception exception)
            {
                if (Tonjiru.Properties.Settings.Default.Notification)
                {
                    NotificationHelper.ShowBalloonTip(exception.Message);
                }
            }
        }

        public App() : base()
        {

        }
    }
}
