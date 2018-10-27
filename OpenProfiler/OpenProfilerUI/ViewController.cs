using System;

using AppKit;
using Foundation;
using System.Threading.Tasks;
using System.Linq;

namespace OpenProfilerUI
{
	public partial class ViewController : NSViewController
	{
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

		private void LaunchProfiler(object sender, EventArgs e)
		{
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
			var selectedPackage = _packagessComboSource.DataSource[(int)PackageCombo.SelectedIndex];
			_selectedPackage = selectedPackage;
			_selectedPackageLaunchActivity = string.Empty;
			if (selectedPackage != null)
			{
				LoadPackageDetails(_selectedDeviceId, selectedPackage);
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
				var packageInfo = await SharedLoader.GetPackageInfoAsync(selectedDeviceId, selectedPackage);
				if (packageInfo != null)
				{
					if (!string.IsNullOrEmpty(packageInfo.Item1) && packageInfo.Item2)
					{
						_selectedPackageLaunchActivity = packageInfo.Item1;
					}
					else
					{
						_selectedPackageLaunchActivity = string.Empty;
					}
				}
			});
		}

		private void DeviceSelected(object sender, EventArgs e)
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

		private void LoadPackages(string deviceId)
		{
			Task.Run(async () =>
			{
				var packages = await SharedLoader.GetPackagesAsync(deviceId);

				InvokeOnMainThread(() =>
				{
					_packagessComboSource.DataSource.Clear();

					foreach (var package in packages)
					{
						_packagessComboSource.DataSource.Add(package);
					}
				});
			});
		}

		private void Initialize()
		{
			Task.Run(async () =>
			{
				var devices = await SharedLoader.GetDevicesAsync();

				InvokeOnMainThread(() =>
				{
					_devicesComboSource.DataSource.Clear();
					foreach (var device in devices)
					{
						_devicesComboSource.DataSource.Add(device);
					}
				});
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
