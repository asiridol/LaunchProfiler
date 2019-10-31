using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace OpenProfiler.ProfileLoaderCore
{
    public class ProfileLoader
    {
        private const string AndroidSdkPathPattern = "/Users/{0}/Library/Developer/Xamarin/android-sdk-macosx";
        ///Applications/Xamarin\ Profiler.app/Contents/MacOS/Xamarin\ Profiler --type=ios --target=/Users/asiri/Projects/mybupa-mobile/Bupa.Mobile.MyBupa.IOS/bin/iPhoneSimulator/Debug/BupaMobileMyBupaIOS.app --device=':v2:runtime=com.apple.CoreSimulator.SimRuntime.iOS-13-1,devicetype=com.apple.CoreSimulator.SimDeviceType.iPhone-11|13.1' --options='name:iPhone 11'

        private const string IosProfilerSimulatorCommandPattern = "{0} --type=ios --target={1} --device=':v2:runtime={2},devicetype={3}|{4}' --options='name:{5}'";
        private const string IosProfilerDeviceCommandPattern = "{0} --type=ios --target={1} --device='{2}' --options=mode:usb";

        private string _loggedInUser = string.Empty;

        public async Task<string[]> GetDevicesAsync()
        {
            string[] devices = null;
            await Task.Run(async () =>
            {
                var loggedInUser = await GetLoggedInUserNameAsync();
                var androidSdkPath = string.Format(AndroidSdkPathPattern, loggedInUser);
                var command = $"{androidSdkPath}/platform-tools/adb -d devices";
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
                var loggedInUser = await GetLoggedInUserNameAsync();
                var androidSdkPath = string.Format(AndroidSdkPathPattern, loggedInUser);
                var command = $"{androidSdkPath}/platform-tools/adb -s {deviceId} shell pm list packages | awk -F \":\" '{{print $2}}'";
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

        public Task LaunchProfilerAsync(string profilerLocation, string deviceId, string packageName, string mainActivityName)
        {
            return Task.Run(() =>
            {
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

        public async Task<string> GetLoggedInUserNameAsync()
        {
            if (string.IsNullOrEmpty(_loggedInUser))
            {
                await Task.Run(async () =>
                {
                    var getUserNameCommand = "id -un";

                    Console.WriteLine("LaunchProfilerAsync : " + getUserNameCommand);

                    var proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = "-c \"" + getUserNameCommand + "\"",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true
                        }
                    };

                    proc.Start();
                    _loggedInUser = await proc.StandardOutput.ReadToEndAsync();
                    _loggedInUser = _loggedInUser.Replace("\n", string.Empty);
                    proc.WaitForExit();
                });
            }

            return _loggedInUser;
        }

        public async Task<List<DeviceDetails>> GetIosDestinationsAsync()
        {
            var allDestinations = new List<DeviceDetails>();
            var simulators = await GetIosSimulatorsAsync();
            var devices = await GetIosDevicesAsync();

            allDestinations.AddRange(devices);
            allDestinations.AddRange(simulators);
            return allDestinations;
        }

        public async Task LaunchProfilerAsync(string profilerLocation, DeviceDetails device, string appPath, string bundleId)
        {
            profilerLocation = profilerLocation + @"/Contents/MacOS/Xamarin Profiler";
            profilerLocation = profilerLocation.Replace(" ", @"\ ");

            string command = string.Empty;
            if (device.IsSimulator)
            {
                var simulator = device as SimulatorDetails;

                var structuredCommand = string.Format(IosProfilerSimulatorCommandPattern,
                    profilerLocation,
                    appPath,
                    simulator.SimRuntime,
                    simulator.DeviceType,
                    device.OSVersion,
                    device.DeviceName);
                command = structuredCommand;
            }
            else
            {
                var deviceNameArg = device.DeviceName;
                var structuredCommand = string.Format(IosProfilerDeviceCommandPattern,
                    profilerLocation,
                    bundleId,
                    deviceNameArg);
                command = structuredCommand;
            }

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
            proc.WaitForExit();
        }

        private async Task<string> GetMainActivityAsync(string deviceId, string packageName)
        {
            string mainActivityName = string.Empty;
            await Task.Run(async () =>
            {
                var loggedInUser = await GetLoggedInUserNameAsync();
                var androidSdkPath = string.Format(AndroidSdkPathPattern, loggedInUser);
                var getMainActivityCommand = $"{androidSdkPath}/platform-tools/adb -s {deviceId} shell dumpsys package {packageName} |grep \"android.intent.action.MAIN:\" -A1 | tail -n 1";
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
                var loggedInUser = await GetLoggedInUserNameAsync();
                var androidSdkPath = string.Format(AndroidSdkPathPattern, loggedInUser);
                var getIsDebuggableCommand = $"{androidSdkPath}/platform-tools/adb -s {deviceId} shell dumpsys package {packageName} |grep pkgFlags";
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

        private async Task<List<DeviceDetails>> GetIosSimulatorsAsync()
        {
            var loggedInUser = await GetLoggedInUserNameAsync();
            var command = $"plutil -convert json -o - /Users/{loggedInUser}/Library/Developer/CoreSimulator/Devices/device_set.plist";
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

            var json = JToken.Parse(processOutput);
            var root = json.Root;

            List<DeviceDetails> simulators = new List<DeviceDetails>();

            try
            {
                var defaultDevicesNode =
                    root.Children().FirstOrDefault(x => x.Path?.Contains("DefaultDevices") ?? false);

                foreach (var child in defaultDevicesNode.Children().Values().TakeWhile(x => !x.Path?.Contains("version") ?? false))
                {
                    var osName = (child as JProperty).Name;

                    foreach (var simulator in child.Values())
                    {
                        var simulatorValue = (simulator as JProperty).Name;


                        var device = new SimulatorDetails
                        {
                            SimRuntime = osName,
                            DeviceType = simulatorValue
                        };

                        simulators.Add(device);
                    }
                }
            }
            catch (Exception ex)
            {
                // do nothing for now
                System.Diagnostics.Debug.WriteLine("Exception occured getting ios simulators");
            }

            Console.WriteLine(processOutput);

            return simulators;
        }
        
        private async Task<List<DeviceDetails>> GetIosDevicesAsync()
        {
            var devices = new List<DeviceDetails>();
            var command = $"instruments -s devices";
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            proc.Start();
            var processOutput = await proc.StandardOutput.ReadToEndAsync();
            proc.WaitForExit();

            var skipFirst2Lines = processOutput.Split("\n").Skip(2).ToList();
            var devicesStrings =
                skipFirst2Lines.TakeWhile(
                    x => !x.Contains("Simulator", StringComparison.InvariantCultureIgnoreCase));

            if (!devicesStrings.Any())
            {
                return devices;
            }

            foreach (var device in devicesStrings)
            {
                var deviceName = device.Substring(0, device.IndexOf("(")).Trim();
                var osStartIndex = device.IndexOf("(");
                var osEndIndex = device.IndexOf(" [");
                var deviceOs = device.Substring(osStartIndex, osEndIndex - osStartIndex);
                deviceOs = deviceOs.Trim('(').Trim(')');
                var deviceDetails = new DeviceDetails
                {
                    DeviceName = deviceName,
                    OSVersion = deviceOs
                };
                devices.Add(deviceDetails);
            }

            return devices;
        }
    }
}
