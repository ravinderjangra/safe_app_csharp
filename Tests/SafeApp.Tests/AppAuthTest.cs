using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SafeApp.Core;

namespace SafeApp.Tests
{
    [TestFixture]
    internal class AppAuthTest
    {
        [Test]
        public async Task ConnectAsRegisteredAppTest()
        {
            var authReq = new AuthReq
            {
                App = new AppExchangeInfo { Id = "net.maidsafe.test", Name = "TestApp", Scope = null, Vendor = "MaidSafe.net Ltd." },
                AppContainer = true,
                Containers = new List<ContainerPermissions>()
            };

            var session = await TestUtils.CreateTestApp(authReq);
            Assert.IsNotNull(session);
        }

        [Test]
        public async Task ConnectAsUnregisteredAppTest()
        {
            var appId = "net.maidsafe.test";
            var session = await Session.AppConnectUnregisteredAsync(appId);
            Assert.IsNotNull(session);
        }
    }
}
