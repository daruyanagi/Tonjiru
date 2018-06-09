using System;

namespace Tonjiru.Models
{
	using System.Diagnostics;
	using System.Runtime.Serialization;

	[DataContract]
	public class WindowInfo : BindableBase
	{
		public TopLevelWindow TopLevelWindow
		{
			get { return topLevelWindow; }
			set { SetProperty(ref topLevelWindow, value); }
		}
		private TopLevelWindow topLevelWindow = null;

		public bool IsTargeted
		{
			get { return isTargeted; }
			set { SetProperty(ref isTargeted, value); }
		}
		private bool isTargeted = true;

		public IntPtr Handle { get { return TopLevelWindow.Handle; } }
		public Process Parent { get { return TopLevelWindow.Parent; } set { topLevelWindow.Parent = value; } }

		[DataMember]
		public string Title
		{
			get { return TopLevelWindow.Title; }
			set { /* dummy for serialize */ }
		}

		[DataMember]
		public string ParentProcessName
		{
			get { return TopLevelWindow.Parent.ProcessName; }
			set { /* dummy for serialize */ }
		}

		[DataMember]
		public bool IsVisible
		{
			get { return TopLevelWindow.IsVisible; }
			set { /* dummy for serialize */ }
		}

		[DataMember]
		public bool IsUwp
		{
			get { return TopLevelWindow.IsUwp; }
			set { /* dummy for serialize */ }
		}

		public void Close()
		{
			TopLevelWindow.Close();
		}
	}
}
