using System;

using AppKit;
using Foundation;
using System.Threading.Tasks;
using System.Linq;

namespace OpenProfilerUI
{
	public partial class ViewController : NSViewController
	{
		public static string ProfilerPathKey = "profiler.launcher.path";

		private ProfileLoader _sharedLoader;
		private ComboBoxDataSource<string> _devicesComboSource;
		private ComboBoxDataSource<string> _packagessComboSource;

		private string _selectedDeviceId;
		private string _selectedPackage;
		private string _selectedPackageLaunchActivity;

		public ViewController(IntPtr handle) : base(handle)
		{
		}

		private ProfileLoader SharedLoader => _sharedLoader ?? (_sharedLoader = new ProfileLoader());



		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			DeviceCombo.Editable = false;
			PackageCombo.Editable = false;

			Initialize();

			_devicesComboSource = new ComboBoxDataSource<string> { GetValue = GetDeviceName, GetIndexOf = GetDeviceIndex };
			DeviceCombo.DataSource = _devicesComboSource;

			_packagessComboSource = new ComboBoxDataSource<string> { GetValue = GetPackageName, GetIndexOf = GetPackageIndex };
			PackageCombo.DataSource = _packagessComboSource;

			using (var nsuserDefaults = new NSUserDefaults())
			{
				var saved = nsuserDefaults.ValueForKey(new NSString(ProfilerPathKey));
				ProfilerPath.StringValue = saved?.ToString() ?? string.Empty;
			}

			ProfilerPath.FocusRingType = DeviceCombo.FocusRingType = PackageCombo.FocusRingType = RefreshButton.FocusRingType = NSFocusRingType.None;

			OverlayView.Material = NSVisualEffectMaterial.Dark;
			OverlayView.BlendingMode = NSVisualEffectBlendingMode.WithinWindow;
			OverlayView.Hidden = true;
		}

		private int GetPackageIndex(string package)
		{
			return _packagessComboSource.DataSource.TakeWhile(x => x != package).Count();
		}

		private string GetPackageName(int position)
		{
			return _packagessComboSource.DataSource[position];
		}

		private string GetDeviceName(int position)
		{
			return _devicesComboSource.DataSource[position];
		}

		private int GetDeviceIndex(string device)
		{
			return _devicesComboSource.DataSource.TakeWhile(x => x != device).Count();
		}

		public override void ViewWillAppear()
		{
			base.ViewWillAppear();

			DeviceCombo.SelectionChanged += DeviceSelected;
			RefreshButton.Activated += DoRefresh;
			PackageCombo.SelectionChanged += PackageSelected;
			LaunchButton.Activated += LaunchProfiler;
		}

		public override void ViewDidDisappear()
		{
			base.ViewDidDisappear();

			DeviceCombo.SelectionChanged -= DeviceSelected;
			RefreshButton.Activated -= DoRefresh;
			PackageCombo.SelectionChanged -= PackageSelected;
			LaunchButton.Activated -= LaunchProfiler;
		}

		public override void ViewWillDisappear()
		{
			base.ViewWillDisappear();

			NSApplication.SharedApplication.Terminate(this);
		}

		private void LaunchProfiler(object sender, EventArgs e)
		{
			if (PackageCombo.SelectedIndex == -1 || DeviceCombo.SelectedIndex == -1)
			{
				return;
			}

			if (string.IsNullOrEmpty(ProfilerPath.StringValue) || !ProfilerPath.StringValue.EndsWith("Xamarin Profiler.app", StringComparison.InvariantCulture))
			{
				NSAlert alert = new NSAlert();
				alert.MessageText = "Not Set";
				alert.InformativeText = "Please set path to profiler first";
				alert.AlertStyle = NSAlertStyle.Critical;
				alert.AddButton("OK");
				alert.BeginSheetForResponse(View.Window, (obj) =>
				{
					alert.Window.Close();
					ProfilerPath.BecomeFirstResponder();
				});

				return;
			}

			var path = ProfilerPath.StringValue;

			using (var nsuserDefaults = new NSUserDefaults())
			{
				nsuserDefaults.SetValueForKey(new NSString(path), new NSString(ProfilerPathKey));
			}

			if (!string.IsNullOrEmpty(_selectedPackageLaunchActivity))
			{
				Task.Run(async () =>
				{
					await SharedLoader.LaunchProfilerAsync(_selectedDeviceId, _selectedPackage, _selectedPackageLaunchActivity);
				});
			}
			else
			{
				var alert = new NSAlert();

				alert.AddButton("OK").Activated += (object s, EventArgs e2) =>
				{
					alert.Window.Close();
					NSApplication.SharedApplication.StopModal();
				};

				alert.InformativeText = "Check if this app is debuggable";
				alert.MessageText = "Can't launch";
				alert.AlertStyle = NSAlertStyle.Critical;

				alert.RunSheetModal(View.Window);
			}
		}

		private void DoRefresh(object sender, EventArgs e)
		{
			Initialize();
		}

		private void PackageSelected(object sender, EventArgs e)
		{
			if (PackageCombo.SelectedIndex > -1)
			{
				var selectedPackage = _packagessComboSource.DataSource[(int)PackageCombo.SelectedIndex];
				_selectedPackage = selectedPackage;
				_selectedPackageLaunchActivity = string.Empty;
				if (selectedPackage != null)
				{
					LoadPackageDetails(_selectedDeviceId, selectedPackage);
				}
			}
			else
			{
				_selectedPackage = null;
			}
		}

		private void LoadPackageDetails(string selectedDeviceId, string selectedPackage)
		{
			if (string.IsNullOrEmpty(selectedDeviceId) || string.IsNullOrEmpty(selectedPackage))
			{
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					InvokeOnMainThread(() =>
					{
						OverlayView.Hidden = false;
						Spinner.StartAnimation(this);
					});

					var mainActivity = await SharedLoader.GetPackageInfoAsync(selectedDeviceId, selectedPackage);
					_selectedPackageLaunchActivity = mainActivity;
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

		private void DeviceSelected(object sender, EventArgs e)
		{
			if (DeviceCombo.SelectedIndex > -1)
			{
				var selected = _devicesComboSource.DataSource[(int)DeviceCombo.SelectedIndex];

				_selectedDeviceId = selected;

				if (selected != null)
				{
					LoadPackages(selected);
				}
				else
				{
					_packagessComboSource.DataSource.Clear();
				}
			}
			else
			{
				_selectedDeviceId = null;
			}
		}

		private void LoadPackages(string deviceId)
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

					var packages = await SharedLoader.GetPackagesAsync(deviceId);

					InvokeOnMainThread(() =>
					{
						_packagessComboSource.DataSource.Clear();

						foreach (var package in packages)
						{
							_packagessComboSource.DataSource.Add(package);
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

					var devices = await SharedLoader.GetDevicesAsync();

					InvokeOnMainThread(() =>
					{
						_devicesComboSource.DataSource.Clear();
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

		public override NSObject RepresentedObject
		{
			get
			{
				return base.RepresentedObject;
			}
			set
			{
				base.RepresentedObject = value;
				// Update the view, if already loaded.
			}
		}
	}
}
