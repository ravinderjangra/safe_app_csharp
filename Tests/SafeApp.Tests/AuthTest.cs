using System.Collections.Generic;
using System.Threading.Tasks;
using SafeApp;
using SafeApp.Core;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SafeAppTests
{
    [Collection("Connect Tests")]
    public class AuthTest
    {
        [Fact]
        public async Task ConnectAsRegisteredAppTest()
        {
            var authReq = new AuthReq
            {
                App = new AppExchangeInfo { Id = "net.maidsafe.test", Name = "TestApp", Scope = null, Vendor = "MaidSafe.net Ltd." },
                AppContainer = true,
                Containers = new List<ContainerPermissions>()
            };

            var session = await TestUtils.CreateTestApp(authReq);
            Assert.NotNull(session);
        }

        [Fact]
        public async Task ConnectAsUnregisteredAppTest()
        {
            var appId = "net.maidsafe.test";
            var session = await Session.AppConnectUnregisteredAsync(appId);
            Assert.NotNull(session);
        }
    }
}
