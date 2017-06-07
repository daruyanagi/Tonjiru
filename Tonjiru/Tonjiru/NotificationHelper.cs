using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonjiru
{
    public static class NotificationHelper
    {
        public static void ShowBalloonTip()
        {
            var notify_icon = new System.Windows.Forms.NotifyIcon();
            notify_icon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().Location);
            notify_icon.BalloonTipTitle = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            notify_icon.BalloonTipText = "Shutdown signals are sent.";
            notify_icon.Visible = true;
            notify_icon.ShowBalloonTip(3000);
        }
    }
}
