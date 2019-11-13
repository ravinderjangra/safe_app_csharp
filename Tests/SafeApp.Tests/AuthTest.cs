using System.Threading.Tasks;
using NUnit.Framework;

namespace SafeApp.Tests
{
    [TestFixture]
    internal class AuthTest
    {
#if (__ANDROID__ || __IOS__) && NON_MOCK_AUTH
        [OneTimeSetUp]
        public async Task SetUp()
        {
            TestUtils.CopyTestAuthResponseFile();
            await TestUtils.TransferVaultConnectionConfigFileAsync();
        }
#endif

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
