# LaunchProfiler
Launch xamarin profiler without enterprise licence

# Pre requisites
## Android
* The project need to be built with debug symbols
* The apk need to be uploaded to the device

## Ios
* Add following entry in to the debug config property group(iOS project .csproj)
`<MtouchProfiling>true</MtouchProfiling>`
* The app needs to be built with debug symbols and Enable Debugging check box turned on in iOS project
* If the target device is a physical device the app need to be deployed to the device

* You could add the following target to inject the property
 ```
 <Target Name="InjectProfilingIOS" BeforeTargets="_CompileToNative" Condition=" '$(Configuration)|$(Platform)' == 'Debug|iPhoneSimulator' Or '$(Configuration)|$(Platform)' == 'Debug|iPhone' ">
    <PropertyGroup>
      <MtouchProfiling>true</MtouchProfiling>
    </PropertyGroup>
  </Target>
  ```
