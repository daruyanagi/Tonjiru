using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Tonjiru
{
    /// <summary>
    /// 多言語化されたリソースと、言語の切り替え機能を提供します。
    /// </summary>
    public class ResourceService : BindableBase
    {
        #region singleton members

        private static readonly ResourceService _current = new ResourceService();
        public static ResourceService Current
        {
            get { return _current; }
        }

        #endregion

        public ResourceService()
        {
            Culture = Tonjiru.Properties.Settings.Default.Language; // 初期化
        }

        private readonly Tonjiru.Properties.Resources _resources = new Tonjiru.Properties.Resources();

        /// <summary>
        /// 多言語化されたリソースを取得します。
        /// </summary>
        public Tonjiru.Properties.Resources Resources
        {
            get { return this._resources; }
        }

        public IEnumerable<string> SupportedCultures
        {
            get
            {
                var path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                path = System.IO.Path.GetDirectoryName(path);

                var dirs = System.IO.Directory.EnumerateDirectories(path).Select(_ => System.IO.Path.GetFileName(_));

                var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(_ => _.ToString());

                return new string[] { "(Default)" } .Concat(dirs.Intersect(cultures));
            }
        }

        public string Culture
        {
            get { return Tonjiru.Properties.Settings.Default.Language; }
            set
            {
                try
                {
                    Tonjiru.Properties.Resources.Culture = CultureInfo.GetCultureInfo(value);
                    Tonjiru.Properties.Settings.Default.Language = value;
                    OnPropertyChanged();
                }
                catch
                {
                    Tonjiru.Properties.Resources.Culture = CultureInfo.GetCultureInfo("en-US");
                    Tonjiru.Properties.Settings.Default.Language = "(Default)";
                    OnPropertyChanged();
                }

                OnPropertyChanged("Resources");
            }
        }
    }
}