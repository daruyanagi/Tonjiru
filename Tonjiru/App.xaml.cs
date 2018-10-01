using System;
using System.Linq;

namespace Tonjiru
{
	using System.IO;
	using System.Runtime.InteropServices;
    using System.Windows.Forms; // 初期設定では参照されていないので追加しておく

    // 修飾しないと System.Windows.Forms.Application に解釈されてコンパイルできない
    public partial class App : System.Windows.Application
    {
        [System.STAThreadAttribute()]
        public static void Main()
        {
            var args = Environment.GetCommandLineArgs();

			// S スイッチ実行または［Shift］キー押し下げ実行で UI less mode
			// そうでなければ UI mode
			if (args.Contains("/s") || (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
			{
				StartUIlessMode();
            }
            else
            {
				StartUIMode();
			}
        }

        public static System.Collections.ObjectModel.ObservableCollection<string> LoadExclusions()
        {
            try
            {
                var exclusions = Tonjiru.Properties.Settings.Default.Exclusions.Split(';');

                return new System.Collections.ObjectModel.ObservableCollection<string>(exclusions);

            }
            catch (Exception exception)
            {
                if (Tonjiru.Properties.Settings.Default.Notification)
                {
                    NotificationHelper.ShowBalloonTip(exception.Message);
                }

                return null;
            }
        }

        private static void StartUIMode()
		{
			var app = new Tonjiru.App();
			app.InitializeComponent();
			app.Run();
		}

		private static void StartUIlessMode()
		{
			try
			{
                var exclusions = LoadExclusions();

                // ホストまで死んじゃうので
                if (!exclusions.Contains("powershell")) exclusions.Add("powershell");
                if (!exclusions.Contains("cmd")) exclusions.Add("cmd");

                foreach (var window in DesktopHelper.GetVisibleWindows())
				{
					var proc_name = window.Parent.ProcessName.ToLower();

					try
					{
						if (exclusions.Contains(proc_name))
						{
							Console.WriteLine($"Skipped: {proc_name}");
							continue;
						}
						else
						{
							Console.WriteLine($"Requested: {proc_name}");
							window.Close();
						}
					}
					catch
					{
						Console.WriteLine($"Failed: {proc_name}");
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error: {e} \n App;ication exited.");
			}
			
			Console.WriteLine($"Done.");
		}

        public App() : base()
        {

        }
    }
}
