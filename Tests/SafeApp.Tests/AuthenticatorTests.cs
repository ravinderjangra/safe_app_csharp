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
            var passphase = TestUtils.GetRandomString(10);
            var password = TestUtils.GetRandomString(10);
            var (_, testCoinKeys) = await Authenticator.AllocateTestCoinsAsync("100");
            var testAuthenticator = await Authenticator.CreateAccountAsync(testCoinKeys.SK, passphase, password);
            Assert.NotNull(testAuthenticator);
            var newAuthenticatorInstance = await Authenticator.LoginAsync(passphase, password);
            Assert.NotNull(newAuthenticatorInstance);
        }

        [Test]
        public async Task AccountErrorTest()
        {
            var passphase = TestUtils.GetRandomString(10);
            var password = TestUtils.GetRandomString(10);

            // Empty or no SafeCoin test
            var (_, testCoinKeys) = await Authenticator.AllocateTestCoinsAsync("0");
            AssertThrows(-207, () => Authenticator.CreateAccountAsync(string.Empty, passphase, password));
            AssertThrows(-100, () => Authenticator.CreateAccountAsync(testCoinKeys.SK, passphase, password));

            var newPassphase = TestUtils.GetRandomString(10);
            var newPassword = TestUtils.GetRandomString(10);
            (_, testCoinKeys) = await Authenticator.AllocateTestCoinsAsync("10");
            var testAuthenticator = await Authenticator.CreateAccountAsync(testCoinKeys.SK, passphase, password);

            // Test create and login API with wrong credentials
            AssertThrows(-100, () => Authenticator.LoginAsync(newPassphase, newPassword));
            AssertThrows(-100, () => Authenticator.LoginAsync(passphase, newPassword));
            AssertThrows(-100, () => Authenticator.CreateAccountAsync(testCoinKeys.SK, passphase, password));
        }

        [Test]
        public async Task AutheriseAppTest()
        {
            var authenticator = await TestUtils.CreateTestAccountAsync();
            var session = await TestUtils.CreateTestApp(authenticator);
            Assert.NotNull(session);
            var authdApps = await authenticator.AuthRegisteredAppsAsync();
            Assert.NotNull(authdApps);
            Assert.NotZero(authdApps.Count);
        }

        [Test]
        public async Task AutheriseAndRevokeAppTest()
        {
            var authenticator = await TestUtils.CreateTestAccountAsync();
            var session = await TestUtils.CreateTestApp(authenticator);
            Assert.NotNull(session);
            var authdApps = await authenticator.AuthRegisteredAppsAsync();
            Assert.NotNull(authdApps);
            Assert.NotZero(authdApps.Count);
            await authenticator.AuthRevokeAppAsync(authdApps[0].Id);
            authdApps = await authenticator.AuthRegisteredAppsAsync();
            Assert.NotNull(authdApps);
            Assert.Zero(authdApps.Count);
        }

        [Test]
        public async Task AuthRequestEncodeDecodeTest()
        {
            var authReq = TestUtils.GenerateAuthRequest();
            var (_, encodedReq) = await Session.EncodeAuthReqAsync(authReq);
            var authenticator = await TestUtils.CreateTestAccountAsync();
            var decodedReq = await authenticator.DecodeIpcMessageAsync(encodedReq);
            Assert.That(decodedReq, Is.TypeOf<AuthIpcReq>());

            var appId = TestUtils.GetRandomString(10);
            (_, encodedReq) = await Session.EncodeUnregisteredRequestAsync(appId);
            decodedReq = await Authenticator.UnRegisteredDecodeIpcMsgAsync(encodedReq);
            Assert.That(decodedReq, Is.TypeOf<UnregisteredIpcReq>());
        }

        void AssertThrows(int errorCode, AsyncTestDelegate func)
        {
            var ex = Assert.ThrowsAsync<FfiException>(func);
            Assert.AreEqual(errorCode, ex.ErrorCode);
        }
    }
}
