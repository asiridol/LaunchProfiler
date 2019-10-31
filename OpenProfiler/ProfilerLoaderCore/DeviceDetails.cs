namespace OpenProfiler.ProfileLoaderCore
{
    public class DeviceDetails
    {
        public virtual bool IsSimulator => false;
        public virtual string OSVersion { get; set; }
        public virtual string DeviceName { get; set; }
    }

    public class SimulatorDetails : DeviceDetails
    {
        public override bool IsSimulator => true;

        public override string OSVersion
        {
            get
            {
                if (string.IsNullOrEmpty(SimRuntime))
                {
                    return string.Empty;
                }

                var startOfOsVersion = SimRuntime.IndexOf("-") + 1;
                var lengthOfOsVersion = SimRuntime.Length - startOfOsVersion;
                var osVersionString = SimRuntime.Substring(startOfOsVersion, lengthOfOsVersion).Replace("-", ".");
                return osVersionString;
            }
        }
        
        public override string DeviceName
        {
            get
            {
                if (string.IsNullOrEmpty(DeviceType))
                {
                    return string.Empty;
                }
                
                var simulatorName = DeviceType.Substring(DeviceType.LastIndexOf(".") + 1,
                    DeviceType.Length - DeviceType.LastIndexOf(".")-1);
                if (simulatorName.EndsWith("-"))
                {
                    simulatorName = simulatorName.TrimEnd('-');
                }

                return simulatorName;
            }
        }
        
        public string SimRuntime { get; set; }
        public string DeviceType { get; set; }
    }
}