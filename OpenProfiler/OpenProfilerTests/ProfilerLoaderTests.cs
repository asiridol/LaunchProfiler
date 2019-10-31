using System.Threading.Tasks;
using NUnit.Framework;
using OpenProfiler.ProfileLoaderCore;

namespace OpenProfilerTests
{
    [TestFixture]
    public class ProfilerLoaderTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task TestLoadDevicesAsync()
        {
            var profilerLoader = new ProfileLoader();
            var devices= await profilerLoader.GetIosDestinationsAsync();
            Assert.IsNotEmpty(devices);
        }
    }
}