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
	[Register ("ViewController")]
	partial class ViewController
	{
		[Outlet]
		AppKit.NSButton BrowseProfilerButton { get; set; }

		[Outlet]
		AppKit.NSComboBox DeviceCombo { get; set; }

		[Outlet]
		AppKit.NSButton LaunchButton { get; set; }

		[Outlet]
		AppKit.NSVisualEffectView OverlayView { get; set; }

		[Outlet]
		AppKit.NSComboBox PackageCombo { get; set; }

		[Outlet]
		AppKit.NSTextField ProfilerPath { get; set; }

		[Outlet]
		AppKit.NSButton RefreshButton { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator Spinner { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (DeviceCombo != null) {
				DeviceCombo.Dispose ();
				DeviceCombo = null;
			}

			if (LaunchButton != null) {
				LaunchButton.Dispose ();
				LaunchButton = null;
			}

			if (OverlayView != null) {
				OverlayView.Dispose ();
				OverlayView = null;
			}

			if (PackageCombo != null) {
				PackageCombo.Dispose ();
				PackageCombo = null;
			}

			if (ProfilerPath != null) {
				ProfilerPath.Dispose ();
				ProfilerPath = null;
			}

			if (RefreshButton != null) {
				RefreshButton.Dispose ();
				RefreshButton = null;
			}

			if (Spinner != null) {
				Spinner.Dispose ();
				Spinner = null;
			}

			if (BrowseProfilerButton != null) {
				BrowseProfilerButton.Dispose ();
				BrowseProfilerButton = null;
			}
		}
	}
}
