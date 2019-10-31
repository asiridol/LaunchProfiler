# LaunchProfiler
Launch xamarin profiler without enterprise licence

# Pre requisites
## Android
* The project need to be built with debug symbols
* The apk need to be uploaded to the device

## Ios
* Add following entry in to the debug config property group(iOS project .csproj)
<MtouchProfiling>true</MtouchProfiling>
* The app need to be built with debug symbols and Enable Debugging check box turned on in iOS project
* If the target device is a physical device the app need to be deployed to the device
