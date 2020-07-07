using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SafeApp.Core;
using SafeAuthenticator;

namespace SafeApp.Tests
{
    [TestFixture]
    public class AuthenticatorTests
    {
        [Test]
        public async Task CreateAccountTest()
        {
            await TestUtils.InitRustLogging();
            var passphase = TestUtils.GetRandomString(10);
            var password = TestUtils.GetRandomString(10);
            var (_, testCoinKeys) = await Session.KeysCreatePreloadTestCoinsAsync("100");
            var testAuthenticator = await Authenticator.CreateAccountAsync(testCoinKeys.SK, passphase, password);
            Assert.NotNull(testAuthenticator);
            var newAuthenticatorInstance = await Authenticator.LoginAsync(passphase, password);
            Assert.NotNull(newAuthenticatorInstance);
        }

        [Test]
        public async Task AutheriseAppTest()
        {
            await TestUtils.InitRustLogging();
            var authenticator = await TestUtils.CreateTestAccountAsync();
            var session = await TestUtils.CreateTestApp(authenticator);
            Assert.NotNull(session);
            var authdApps = await authenticator.AuthRegisteredAppsAsync();
            Assert.NotNull(authdApps);
            Assert.NotZero(authdApps.Count);
        }
    }
}
