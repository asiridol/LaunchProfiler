// This file has been autogenerated from a class added in the UI designer.

using System;

using Foundation;
using AppKit;
using System.Linq;
using System.Threading.Tasks;
using OpenProfiler.ProfileLoaderCore;

namespace OpenProfilerUI
{
    public partial class IosViewController : NSViewController
    {
        public static string ProfilerPathKey = "profiler.launcher.path";
        private const string LastBundleIdKey = "profiler.launch.bundleId";

        private ProfileLoader _sharedLoader;
        private ComboBoxDataSource<DeviceDetails> _devicesComboSource;

        public IosViewController(IntPtr handle) : base(handle)
        {
        }

        private ProfileLoader SharedLoader => _sharedLoader ?? (_sharedLoader = new ProfileLoader());

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "Ios profile loader";

            DestinationsComboBox.Editable = false;

            Initialize();

            _devicesComboSource = new ComboBoxDataSource<DeviceDetails> { GetValue = GetDeviceName, GetIndexOf = GetDeviceIndex };
            DestinationsComboBox.DataSource = _devicesComboSource;

            using (var nsuserDefaults = new NSUserDefaults())
            {
                var saved = nsuserDefaults.ValueForKey(new NSString(ProfilerPathKey));
                PathToProfilerText.StringValue = saved?.ToString() ?? string.Empty;

                var savedBundleId = nsuserDefaults.ValueForKey(new NSString(LastBundleIdKey));
                BundleIdText.StringValue = savedBundleId?.ToString() ?? string.Empty;
            }

            PathToProfilerText.FocusRingType =
                DestinationsComboBox.FocusRingType =
                AppPathText.FocusRingType =
                RefreshDevicesButton.FocusRingType =
                BundleIdText.FocusRingType = NSFocusRingType.None;

            OverlayView.Material = NSVisualEffectMaterial.Dark;
            OverlayView.BlendingMode = NSVisualEffectBlendingMode.WithinWindow;
            OverlayView.Hidden = true;
        }

        public override void ViewWillAppear()
        {
            base.ViewWillAppear();

            RefreshDevicesButton.Activated += OnRefreshButtonClicked;
            LaunchProfilerButton.Activated += OnLaunchProfileClicked;
            BrowseAppButton.Activated += OnBrowseAppButtonClicked;
            BrowseProfiler.Activated += OnBrowseProfilerButtonClicked;
        }

        public override void ViewDidDisappear()
        {
            RefreshDevicesButton.Activated -= OnRefreshButtonClicked;
            LaunchProfilerButton.Activated -= OnLaunchProfileClicked;
            BrowseAppButton.Activated -= OnBrowseAppButtonClicked;
            BrowseProfiler.Activated -= OnBrowseProfilerButtonClicked;
            base.ViewDidDisappear();
        }

        private void OnBrowseProfilerButtonClicked(object sender, EventArgs e)
        {
            var browseDialog = NSOpenPanel.OpenPanel;
            browseDialog.CanChooseDirectories = false;
            browseDialog.CanChooseFiles = true;
            browseDialog.AllowedFileTypes = new string[] { "app" };

            if (browseDialog.RunModal() == 1)
            {
                var url = browseDialog.Urls[0];
                if (url != null)
                {
                    PathToProfilerText.StringValue = url.Path;
                }
            }
        }

        private void OnBrowseAppButtonClicked(object sender, EventArgs e)
        {
            var browseDialog = NSOpenPanel.OpenPanel;
            browseDialog.CanChooseDirectories = false;
            browseDialog.CanChooseFiles = true;
            browseDialog.AllowedFileTypes = new string[] { "app" };

            if (browseDialog.RunModal() == 1)
            {
                var url = browseDialog.Urls[0];
                if (url != null)
                {
                    AppPathText.StringValue = url.Path;
                }
            }
        }

        private void OnLaunchProfileClicked(object sender, EventArgs e)
        {
            LaunchApp();
        }

        private void LaunchApp()
        {
            if (DestinationsComboBox.SelectedIndex == -1)
            {
                return;
            }

            if (string.IsNullOrEmpty(AppPathText.StringValue) || !AppPathText.StringValue.EndsWith(".app", StringComparison.InvariantCultureIgnoreCase))
            {
                NSAlert alert = new NSAlert();
                alert.MessageText = "Not Set";
                alert.InformativeText = "Please set path to app first";
                alert.AlertStyle = NSAlertStyle.Critical;
                alert.AddButton("OK");
                alert.BeginSheetForResponse(View.Window, (obj) =>
                {
                    alert.Window.Close();
                    AppPathText.BecomeFirstResponder();
                });

                return;
            }

            if (string.IsNullOrEmpty(BundleIdText.StringValue))
            {
                NSAlert alert = new NSAlert();
                alert.MessageText = "Not Set";
                alert.InformativeText = "Please set bundle id of app first";
                alert.AlertStyle = NSAlertStyle.Critical;
                alert.AddButton("OK");
                alert.BeginSheetForResponse(View.Window, (obj) =>
                {
                    alert.Window.Close();
                    BundleIdText.BecomeFirstResponder();
                });

                return;
            }

            if (string.IsNullOrEmpty(PathToProfilerText.StringValue) || !PathToProfilerText.StringValue.EndsWith("Xamarin Profiler.app", StringComparison.InvariantCulture))
            {
                NSAlert alert = new NSAlert();
                alert.MessageText = "Not Set";
                alert.InformativeText = "Please set path to profiler first";
                alert.AlertStyle = NSAlertStyle.Critical;
                alert.AddButton("OK");
                alert.BeginSheetForResponse(View.Window, (obj) =>
                {
                    alert.Window.Close();
                    PathToProfilerText.BecomeFirstResponder();
                });

                return;
            }

            var profilerPath = PathToProfilerText.StringValue;
            var selectedDevice = _devicesComboSource.DataSource.ElementAt((int)DestinationsComboBox.SelectedIndex);
            var appPath = AppPathText.StringValue;
            var bundleId = BundleIdText.StringValue;

            using (var nsuserDefaults = new NSUserDefaults())
            {
                nsuserDefaults.SetValueForKey(new NSString(profilerPath), new NSString(ProfilerPathKey));
                nsuserDefaults.SetValueForKey(new NSString(bundleId), new NSString(LastBundleIdKey));
            }

            Task.Run(async () =>
            {
                await SharedLoader.LaunchProfilerAsync(profilerPath, selectedDevice, appPath, bundleId);
            });
        }

        private void OnRefreshButtonClicked(object sender, EventArgs e)
        {
            Initialize();
        }

        private int GetDeviceIndex(string displayText)
        {
            if (string.IsNullOrEmpty(displayText))
            {
                return -1;
            }

            var indexOfPipe = displayText.IndexOf("|");
            var namePart = displayText.Substring(0, displayText.IndexOf("|"));
            var osPart = displayText.Substring(namePart.Length, displayText.Length - 1 - namePart.Length);
            var item = _devicesComboSource.DataSource.FirstOrDefault(x => x.DeviceName.Equals(namePart) && x.OSVersion.Equals(osPart));
            var position = _devicesComboSource.DataSource.IndexOf(item);
            return position;
        }

        private string GetDeviceName(int position)
        {
            var element = _devicesComboSource.DataSource.ElementAt(position);
            return element.DeviceName + "|" + element.OSVersion;
        }

        private void Initialize()
        {
            Task.Run(async () =>
            {
                try
                {
                    InvokeOnMainThread(() =>
                    {
                        OverlayView.Hidden = false;
                        Spinner.StartAnimation(this);
                    });

                    var devices = await SharedLoader.GetIosDestinationsAsync();

                    InvokeOnMainThread(() =>
                    {
                        _devicesComboSource.DataSource.Clear();

                        if (devices == null)
                        {
                            return;
                        }
                        foreach (var device in devices)
                        {
                            _devicesComboSource.DataSource.Add(device);
                        }
                    });

                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        OverlayView.Hidden = true;
                        Spinner.StopAnimation(this);
                    });
                }
            });
        }
    }
}
