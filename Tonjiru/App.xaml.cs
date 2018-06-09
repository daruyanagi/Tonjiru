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

        public static string GetPathOfProcessExclusions()
        {
            var path = System.Reflection.Assembly.GetEntryAssembly().Location;

            path = System.IO.Path.GetDirectoryName(path);
            path = System.IO.Path.Combine(path, "exclusions.txt");

            if (System.IO.File.Exists(path)) return path;

            throw new Exception("Not find exclusions.txt");
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
				var exclusions = File.ReadAllLines(App.GetPathOfProcessExclusions());

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
