// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace OpenProfilerUI
{
	[Register("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSComboBox DeviceCombo { get; set; }

		[Outlet]
		AppKit.NSButton LaunchButton { get; set; }

		[Outlet]
		AppKit.NSComboBox PackageCombo { get; set; }

		[Outlet]
		AppKit.NSButton RefreshButton { get; set; }

		void ReleaseDesignerOutlets()
		{
			if (DeviceCombo != null)
			{
				DeviceCombo.Dispose();
				DeviceCombo = null;
			}

			if (LaunchButton != null)
			{
				LaunchButton.Dispose();
				LaunchButton = null;
			}

			if (PackageCombo != null)
			{
				PackageCombo.Dispose();
				PackageCombo = null;
			}

			if (RefreshButton != null)
			{
				RefreshButton.Dispose();
				RefreshButton = null;
			}
		}
	}
}
