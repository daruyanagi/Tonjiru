using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Tonjiru
{
    using System.Windows.Forms; // 初期設定では参照されていないので追加しておく

    // 修飾しないと System.Windows.Forms.Application に解釈されてコンパイルできない
    public partial class App : System.Windows.Application 
    {
        public App() : base()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                MainWindow = new Views.MainWindow();
                MainWindow.Show();
            }
            else
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
                    Console.WriteLine($"Error: {exception.Message}");
                    Console.WriteLine($"Press Any Key To Exit");
                    Console.Read();
                }
                finally
                {
                    Shutdown();
                }
            }
        }
    }
}
