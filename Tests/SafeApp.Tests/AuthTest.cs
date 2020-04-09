using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SafeApp.Core;

namespace SafeApp.Tests
{
    [TestFixture]
    internal class AuthTest
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
            await TestUtils.InitRustLogging();
            var autoResetEvent = new AutoResetEvent(false);
            var wasCalled = false;
            var sw = new Stopwatch();
            sw.Start();
            var appId = "net.maidsafe.test";
            var session = await Session.AppConnectUnregisteredAsync(appId);
            Session.Disconnected += (s, e) =>
            {
                wasCalled = true;
                autoResetEvent.Set();
            };
            sw.Stop();
            await Task.Delay(200000);
            Assert.IsFalse(autoResetEvent.WaitOne(200000));
            Assert.IsNotNull(session);
            Assert.IsTrue(wasCalled);
        }
    }
}
