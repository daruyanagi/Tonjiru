using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonjiru.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;
	using System.Runtime.Serialization.Json;
	using Tonjiru.Models;

	public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel()
        {
			InitializeCommands();
			LoadExclusions();

			RefreshCommand.Execute(null); // コンストラクターだけど非同期実行したいのでコマンド経由で呼ぶ
        }

        ~MainWindowViewModel()
        {
			SaveExclusions();
        }

		private void InitializeCommands()
		{
			string GetWindowsInfoByText()
			{
                if (WindowsInfoList == null) return string.Empty;

				var serializer = new DataContractJsonSerializer(typeof(List<WindowInfo>));

				using (var stream = new System.IO.MemoryStream())
				using (var reader = new System.IO.StreamReader(stream))
				{
					serializer.WriteObject(stream, WindowsInfoList.ToList());
					stream.Position = 0;
					return reader.ReadToEnd();
				}
			};

            RefreshCommand = new RelayCommand(async () =>
            {
                RefreshCommandCanExecute = false;

                await RefreshVisibleWindowsAsync();

                RefreshCommandCanExecute = true;
            });

			CopyCommand = new RelayCommand(() =>
			{
				System.Windows.Clipboard.SetText(GetWindowsInfoByText());
			});

			SaveCommand = new RelayCommand(() =>
			{
				using (var dialog = new System.Windows.Forms.SaveFileDialog())
				{
					dialog.Filter = "Plain Text|*.txt";

					switch (dialog.ShowDialog())
					{
						case System.Windows.Forms.DialogResult.OK:
							System.IO.File.WriteAllText(dialog.FileName, GetWindowsInfoByText());
							break;
					}
				}
			});

			CloseAllWindowsCommand = new RelayCommand(async () =>
			{
				CloseAllWindows();

				await RefreshVisibleWindowsAsync();
			});

			CloseAllWindowsAndExitCommand = new RelayCommand(() =>
			{
				CloseAllWindows();

				App.Current.Shutdown();
			});

			AddExclusionsCommand = new RelayCommand<WindowInfo>(_ =>
			{
				var process_name = _.Parent.ProcessName.ToLower();
				if (Exclusions.IndexOf(process_name) < 0)
					Exclusions.Add(process_name);

				// 既存のウィンドウリストにあるエントリからも
				// ［閉じる］チェックを外しておく親切設計
				foreach (var window in WindowsInfoList)
				{
					if (window.Parent.ProcessName.ToLower() == process_name)
						window.IsTargeted = false;
				}

				// 保存するの忘れてたぜ！（v1.3 まで
				SaveExclusions();

				OnPropertyChanged(nameof(Exclusions));
			});

			RemoveExclusionsCommand = new RelayCommand<string>(_ =>
			{
				Exclusions.Remove(_);

				// 保存するの忘れてたぜ！（v1.3 まで
				SaveExclusions();

				OnPropertyChanged(nameof(Exclusions));
			});
		}

        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
		public RelayCommand CloseAllWindowsCommand { get; private set; }
		public RelayCommand CloseAllWindowsAndExitCommand { get; private set; }
		public RelayCommand<WindowInfo> AddExclusionsCommand { get; private set; }
        public RelayCommand<string> RemoveExclusionsCommand { get; private set; }

		public void CloseAllWindows()
		{
            if (WindowsInfoList == null) return;

            foreach (var window in WindowsInfoList)
			{
				var proc_name = window.Parent.ProcessName.ToLower();

				if (window.IsTargeted)
				{
					foreach (var process in Process.GetProcessesByName(proc_name))
					{
						try
						{
							process.Kill();

							Console.WriteLine($"Killed: {proc_name}");
						}
						catch (Exception exception)
						{
							Console.WriteLine($"Failed: {proc_name} ({exception.Message})");
						}
					}
				}
				else
				{
					Console.WriteLine($"Skipped: {proc_name}");
				}
			}

			if (Properties.Settings.Default.Notification)
			{
				NotificationHelper.ShowBalloonTip();
			}
		}

		public void LoadExclusions()
		{
			try
			{
				var exclusions = System.IO.File.ReadAllLines(App.GetPathOfProcessExclusions());

				Exclusions = new ObservableCollection<string>(exclusions);
			}
			catch (Exception exception)
			{
				if (Tonjiru.Properties.Settings.Default.Notification)
				{
					NotificationHelper.ShowBalloonTip(exception.Message);
				}

				App.Current.Shutdown(-1);
			}
		}

		public void SaveExclusions()
		{
			System.IO.File.WriteAllLines(App.GetPathOfProcessExclusions(), Exclusions);
		}

		public async Task RefreshVisibleWindowsAsync()
		{
			await Task.Run(() => RefreshVisibleWindows());
		}

		public void RefreshVisibleWindows()
        {
            LoadingPanelVisibiliry = System.Windows.Visibility.Visible;

            var temp = DesktopHelper
                .GetVisibleWindows()
                .Select(_ => new WindowInfo()
                {
                    TopLevelWindow = _,
                    IsTargeted = Exclusions.IndexOf(_.Parent?.ProcessName.ToLower()) < 0,
                });

            WindowsInfoList = new ObservableCollection<WindowInfo>(temp);
			
            OnPropertyChanged(nameof(WindowsInfoList));

            LoadingPanelVisibiliry = System.Windows.Visibility.Collapsed;
        }

        public ObservableCollection<WindowInfo> WindowsInfoList { get; set; }

        public ObservableCollection<string> Exclusions { get; set; }

        public System.Windows.Visibility LoadingPanelVisibiliry
        {
            get => loadingPanelVisibiliry;
            set => SetProperty(ref loadingPanelVisibiliry, value);
        }
        private System.Windows.Visibility loadingPanelVisibiliry = System.Windows.Visibility.Collapsed;

        public bool RefreshCommandCanExecute
        {
            get => refreshCommandCanExecute;
            set => SetProperty(ref refreshCommandCanExecute, value);
        }
        private bool refreshCommandCanExecute = true;

        public string AppNameAndVersion
        {
            get
            {
                var info = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                return $"{info.Name} v{info.Version}";
            }
        }
    }
}
