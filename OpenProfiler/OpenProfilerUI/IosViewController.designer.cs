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
	[Register ("IosViewController")]
	partial class IosViewController
	{
		[Outlet]
		AppKit.NSTextField AppPathText { get; set; }

		[Outlet]
		AppKit.NSButton BrowseAppButton { get; set; }

		[Outlet]
		AppKit.NSButton BrowseProfiler { get; set; }

		[Outlet]
		AppKit.NSTextField BundleIdText { get; set; }

		[Outlet]
		AppKit.NSComboBox DestinationsComboBox { get; set; }

		[Outlet]
		AppKit.NSButton LaunchProfilerButton { get; set; }

		[Outlet]
		AppKit.NSVisualEffectView OverlayView { get; set; }

		[Outlet]
		AppKit.NSTextField PathToProfilerText { get; set; }

		[Outlet]
		AppKit.NSButton RefreshDevicesButton { get; set; }

		[Outlet]
		AppKit.NSProgressIndicator Spinner { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BrowseProfiler != null) {
				BrowseProfiler.Dispose ();
				BrowseProfiler = null;
			}

			if (BrowseAppButton != null) {
				BrowseAppButton.Dispose ();
				BrowseAppButton = null;
			}

			if (AppPathText != null) {
				AppPathText.Dispose ();
				AppPathText = null;
			}

			if (BundleIdText != null) {
				BundleIdText.Dispose ();
				BundleIdText = null;
			}

			if (DestinationsComboBox != null) {
				DestinationsComboBox.Dispose ();
				DestinationsComboBox = null;
			}

			if (LaunchProfilerButton != null) {
				LaunchProfilerButton.Dispose ();
				LaunchProfilerButton = null;
			}

			if (OverlayView != null) {
				OverlayView.Dispose ();
				OverlayView = null;
			}

			if (PathToProfilerText != null) {
				PathToProfilerText.Dispose ();
				PathToProfilerText = null;
			}

			if (RefreshDevicesButton != null) {
				RefreshDevicesButton.Dispose ();
				RefreshDevicesButton = null;
			}

			if (Spinner != null) {
				Spinner.Dispose ();
				Spinner = null;
			}
		}
	}
}
