using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonjiru.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel()
        {
            RefreshCommand = new RelayCommand(() => { RefreshVisibleWindows(); });

            CloseAllWindowsAndExitCommand = new RelayCommand(() => 
            {
                try
                {
                    foreach (var window in Windows)
                    {
                        if (window.IsTargeted)
                        {
                            window.Close();
                        }
                    }

                    if (Properties.Settings.Default.Notification)
                    {
                        NotificationHelper.ShowBalloonTip();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error: {exception.Message}");
                    Console.WriteLine($"Press Any Key To Exit");
                    Console.ReadKey();
                }
                finally
                {
                    App.Current.Shutdown();
                }
            });

            AddExclusionsCommand = new RelayCommand<WindowInfo>(_ =>
            {
                var process_name = _.Parent.ProcessName.ToLower();
                if (Exclusions.IndexOf(process_name) < 0)
                Exclusions.Add(process_name);

                // ［閉じる］チェックを外しておく親切設計
                foreach (var window in Windows)
                {
                    if (window.Parent.ProcessName.ToLower() == process_name)
                        window.IsTargeted = false;
                }
            });

            RemoveExclusionsCommand = new RelayCommand<string>(_ =>
            {
                Exclusions.Remove(_);
                OnPropertyChanged("Exclusions");
            });

            RefreshVisibleWindows();
        }

        ~MainWindowViewModel()
        {
            System.IO.File.WriteAllLines("exclusions.txt", Exclusions);
        }

        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand CloseAllWindowsAndExitCommand { get; private set; }
        public RelayCommand<WindowInfo> AddExclusionsCommand { get; private set; }
        public RelayCommand<string> RemoveExclusionsCommand { get; private set; }

        public void RefreshVisibleWindows()
        {
            Exclusions = new ObservableCollection<string>(System.IO.File.ReadAllLines("exclusions.txt"));

            var temp = DesktopHelper
                .GetVisibleWindows()
                .Select(_ => new WindowInfo()
                {
                    TopLevelWindow = _,
                    IsTargeted = Exclusions.IndexOf(_.Parent?.ProcessName.ToLower()) < 0,
                });

            Windows = new ObservableCollection<WindowInfo>(temp);

            OnPropertyChanged("Exclusions");
            OnPropertyChanged("Windows");
        }

        public ObservableCollection<WindowInfo> Windows { get; set; }

        public ObservableCollection<string> Exclusions { get; set; }

        public string AppNameAndVersion
        {
            get
            {
                var info = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                return $"{info.Name} v{info.Version}";
            }
        }
    }

    public class WindowInfo : BindableBase
    {
        private TopLevelWindow topLevelWindow = null;
        public TopLevelWindow TopLevelWindow
        {
            get { return topLevelWindow; }
            set { SetProperty(ref topLevelWindow, value); }
        }

        private bool isTargeted = true;
        public bool IsTargeted
        {
            get { return isTargeted; }
            set { SetProperty(ref isTargeted, value); }
        }

        public IntPtr Handle { get { return TopLevelWindow.Handle; } }
        public string Title { get { return TopLevelWindow.Title; } }
        public Process Parent { get { return TopLevelWindow.Parent; } }
        public bool IsVisible { get { return TopLevelWindow.IsVisible; } }

        public void Close()
        {
            TopLevelWindow.Close();
        }
    }
}
