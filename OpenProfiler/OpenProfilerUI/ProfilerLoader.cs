using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Foundation;
using System.Collections.Generic;

namespace OpenProfilerUI
{
	public class ProfileLoader
	{
		private const string AndroidSdkPath = "/Users/asiri/Library/Developer/Xamarin/android-sdk-macosx";

		private string ProfilerLocation
		{
			get
			{
				var path = string.Empty;
				using (var nsuserDefaults = new NSUserDefaults())
				{
					var saved = nsuserDefaults.ValueForKey(new NSString(ViewController.ProfilerPathKey));
					path = saved.ToString();
				}

				return path;
			}
		}

		public async Task<string[]> GetDevicesAsync()
		{
			string[] devices = null;
			await Task.Run(async () =>
			{
				var command = $"{AndroidSdkPath}/platform-tools/adb -d devices";
				Console.WriteLine("GetDevicesAsync : " + command);
				var proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "/bin/bash",
						Arguments = "-c \"" + command + "\"",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};

				proc.Start();
				var processOutput = await proc.StandardOutput.ReadToEndAsync();
				proc.WaitForExit();

				Console.WriteLine("GetDevicesAsync : " + processOutput);

				if (!string.IsNullOrEmpty(processOutput))
				{
					var sanitizedString = processOutput.Replace("List of devices attached", string.Empty).Replace("\tdevice", string.Empty);
					devices = sanitizedString.Split("\n").Where(x => !string.IsNullOrEmpty(x)).ToArray();
				}
			});

			return devices;
		}

		public async Task<string[]> GetPackagesAsync(string deviceId)
		{
			string[] packages = null;
			await Task.Run(async () =>
			{
				var command = $"{AndroidSdkPath}/platform-tools/adb -s {deviceId} shell pm list packages | awk -F \":\" '{{print $2}}'";
				Console.WriteLine("GetPackagesAsync : " + command);
				var proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "/bin/bash",
						Arguments = "-c \"" + command + "\"",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};

				proc.Start();
				var processOutput = await proc.StandardOutput.ReadToEndAsync();
				proc.WaitForExit();

				Console.WriteLine("GetPackagesAsync : " + processOutput);

				var interimPackages = new string[0];

				if (!string.IsNullOrEmpty(processOutput))
				{
					interimPackages = processOutput.Split("\n").TakeWhile(x => !string.IsNullOrEmpty(x)).OrderBy(x => x).ToArray();
				}

				var tasks = new List<Task<Tuple<string, bool>>>();

				foreach (var package in interimPackages)
				{
					tasks.Add(GetIsDebuggableAsync(deviceId, package));
				}

				var results = await Task.WhenAll(tasks);
				packages = results.Where(x => x.Item2).Select(x => x.Item1).ToArray();
			});

			return packages;
		}

		public async Task<string> GetPackageInfoAsync(string deviceId, string packageName)
		{
			var mainActivity = await GetMainActivityAsync(deviceId, packageName);
			return mainActivity;
		}

		public Task LaunchProfilerAsync(string deviceId, string packageName, string mainActivityName)
		{
			return Task.Run(() =>
			{
				var profilerLocation = ProfilerLocation;
				profilerLocation = profilerLocation.Replace(" ", "\\ ");
				var launchProfilerCommand = $"{profilerLocation}/Contents/MacOS/Xamarin\\ Profiler --type=android --device={deviceId} --target={packageName}\\|{mainActivityName}";

				Console.WriteLine("LaunchProfilerAsync : " + launchProfilerCommand);

				var proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "/bin/bash",
						Arguments = "-c \"" + launchProfilerCommand + "\"",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};

				proc.Start();
				proc.WaitForExit();
			});
		}

		private async Task<string> GetMainActivityAsync(string deviceId, string packageName)
		{
			string mainActivityName = string.Empty;
			await Task.Run(async () =>
			{
				var getMainActivityCommand = $"{AndroidSdkPath}/platform-tools/adb -s {deviceId} shell dumpsys package {packageName} |grep \"android.intent.action.MAIN:\" -A1 | tail -n 1";
				Console.WriteLine("GetMainActivityAsync : " + getMainActivityCommand);

				var proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "/bin/bash",
						Arguments = "-c \"" + getMainActivityCommand + "\"",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};

				proc.Start();
				var processOutput = await proc.StandardOutput.ReadToEndAsync();
				proc.WaitForExit();

				Console.WriteLine("GetMainActivityAsync : " + processOutput);

				if (!string.IsNullOrEmpty(processOutput))
				{
					var lookUpString = $"{packageName}/";
					mainActivityName = processOutput.Substring(processOutput.IndexOf(lookUpString) + lookUpString.Length).Split(" ").FirstOrDefault();
				}
			});

			return mainActivityName;
		}

		private async Task<Tuple<string, bool>> GetIsDebuggableAsync(string deviceId, string packageName)
		{
			bool isDebuggable = false;

			await Task.Run(async () =>
			{
				var getIsDebuggableCommand = $"{AndroidSdkPath}/platform-tools/adb -s {deviceId} shell dumpsys package {packageName} |grep pkgFlags";
				Console.WriteLine("GetIsDebuggableAsync : " + getIsDebuggableCommand);
				var proc = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "/bin/bash",
						Arguments = "-c \"" + getIsDebuggableCommand + "\"",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};

				proc.Start();
				var processOutput = await proc.StandardOutput.ReadToEndAsync();
				proc.WaitForExit();

				Console.WriteLine("GetIsDebuggableAsync : " + processOutput);

				isDebuggable = !string.IsNullOrEmpty(processOutput) && processOutput.Contains("DEBUGGABLE");
			});

			return new Tuple<string, bool>(packageName, isDebuggable);
		}
	}
}
