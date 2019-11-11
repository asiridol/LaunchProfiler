using System;
using AppKit;
using Foundation;
using OpenProfilerForms;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace OpenProfilerUI
{
	[Register("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate
    {
        private readonly NSWindow _window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            _window.Title = "Xamarin.Forms on Mac!"; // choose your own Title here
            _window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            _window.WillClose += OnWIndowClose;
        }

        private void OnWIndowClose(object sender, EventArgs e)
        {
            NSApplication.SharedApplication.Terminate(this);
        }

        public override NSWindow MainWindow
        {
            get { return _window; }
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
        }
    }
}
