using System.Threading.Tasks;
using NUnit.Framework;

namespace SafeApp.Tests
{
    [TestFixture]
    internal class AuthTest
    {
        [Test]
        public async Task ConnectAsRegisteredAppTest()
        {
            var session = await TestUtils.CreateTestApp();
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
