﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tonjiru.ViewModel
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.Serialization;

    public class MainWindowViewModel : BindableBase
    {
        [DataContract]
        public class Window
        {
            [DataMember(Name = "title")]
            public string Title { get; set; }

            [DataMember(Name = "processName")]
            public string ProcessName { get; set; }

            [DataMember(Name = "isTargeted")]
            public bool IsTargeted { get; set; }
        }

        public MainWindowViewModel()
        {
            RefreshCommand = new RelayCommand(() => { RefreshVisibleWindows(); });

            string GetWindowsInfoByText()
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(List<Window>));

                using (var stream = new System.IO.MemoryStream())
                using (var reader = new System.IO.StreamReader(stream))
                {
                    serializer.WriteObject(stream, Windows.Select(_ => new Window()
                    {
                        Title = _.Title,
                        ProcessName = _.Parent.ProcessName.ToLower(),
                        IsTargeted = _.IsTargeted
                    }).ToList());
                    stream.Position = 0;
                    return reader.ReadToEnd();
                }
            };

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
        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CloseAllWindowsAndExitCommand { get; private set; }
        public RelayCommand<WindowInfo> AddExclusionsCommand { get; private set; }
        public RelayCommand<string> RemoveExclusionsCommand { get; private set; }

        public void RefreshVisibleWindows()
        {
            try
            {
                var exclusions = System.IO.File.ReadAllLines("exclusions.txt");

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
