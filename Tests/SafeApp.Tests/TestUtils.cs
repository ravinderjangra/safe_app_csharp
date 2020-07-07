using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SafeApp.Core;
using SafeAuthenticator;

#if __ANDROID__
using Android.App;
#endif

namespace SafeApp.Tests
{
    public static class TestUtils
    {
        public static readonly Random Random = new Random();

        public static async Task<Authenticator> CreateTestAccountAsync()
        {
            var (_, testCoinKeys) = await Session.KeysCreatePreloadTestCoinsAsync("100");
            var passphase = GetRandomString(10);
            var password = GetRandomString(10);
            var authenticator = await Authenticator.CreateAccountAsync(testCoinKeys.SK, passphase, password);
            Assert.NotNull(authenticator);
            return authenticator;
        }

        public static async Task<string> AuthenticateAuthRequestAsync(Authenticator authenticator, AuthReq authReq, bool allow)
        {
            var (_, reqMsg) = await Session.EncodeAuthReqAsync(authReq);
            return await authenticator.AutheriseAppAsync(reqMsg, allow);
        }

        private static Task<string> AuthenticateAuthRequestAsync(Authenticator authenticator, string ipcMsg, bool allow)
        {
            return authenticator.AutheriseAppAsync(ipcMsg, allow);
        }

        private static AuthReq GenerateAuthRequest()
        {
            return new AuthReq
            {
                App = new AppExchangeInfo
                {
                    Id = GetRandomString(10),
                    Name = GetRandomString(5),
                    Scope = null,
                    Vendor = GetRandomString(5)
                },
                AppContainer = true,
                AppPermissionTransferCoins = true,
                AppPermissionGetBalance = true,
                AppPermissionPerformMutations = true,
                Containers = new List<ContainerPermissions>()
            };
        }

        public static Task<Session> CreateTestApp()
        {
            var authReq = GenerateAuthRequest();
            return CreateTestApp(authReq);
        }

        public static async Task<Session> CreateTestApp(AuthReq authReq)
        {
            var authenticator = await CreateTestAccountAsync();
            var resMsg = await AuthenticateAuthRequestAsync(authenticator, authReq, true);
            return await Session.AppConnectAsync(authReq.App.Id, resMsg);
        }

        public static async Task<Session> CreateTestApp(Authenticator authenticator)
        {
            var authReq = GenerateAuthRequest();
            var resMsg = await AuthenticateAuthRequestAsync(authenticator, authReq, true);
            return await Session.AppConnectAsync(authReq.App.Id, resMsg);
        }

        public static byte[] GenerateRandomXorName()
        {
            var xorName = new byte[AppConstants.XorNameLen];
            Random.NextBytes(xorName);
            return xorName;
        }

        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static async Task<string> InitRustLogging()
        {
#if __IOS__
            var configPath = Environment.GetFolderPath(Environment.SpecialFolder.Resources);
            using (var reader = new StreamReader(Path.Combine(".", "log.toml")))
            {
#elif __ANDROID__
            var configPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            using (var reader = new StreamReader(Application.Context.Assets.Open("log.toml")))
            {
#else
            var configPath = Path.Combine(Path.GetTempPath(), GetRandomString(8));
            Directory.CreateDirectory(configPath);
            var srcPath = Path.Combine(Directory.GetParent(typeof(MiscTest).Assembly.Location).FullName, "log.toml");
            using (var reader = new StreamReader(srcPath))
            {
#endif
                using (var writer = new StreamWriter(Path.Combine(configPath, "log.toml")))
                {
                    writer.Write(reader.ReadToEnd());
                    writer.Close();
                }

                reader.Close();
            }

            await Session.SetAppConfigurationDirectoryPathAsync(configPath);
            await Session.InitLoggingAsync();
            return configPath;
        }

        public static void PrepareTestData()
        {
            Directory.CreateDirectory(TestDataDir);
            var testFilePath = Path.Combine(TestDataDir, "index.html");
            File.WriteAllText(testFilePath, GetRandomString(20));
        }

        public static void RemoveTestData()
            => Directory.Delete(TestDataDir, true);

        public static string TestDataDir =>
#if __ANDROID__
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), _testDataDir);
#else
                _testDataDir;
#endif

        static readonly string _testDataDir = TestUtils.GetRandomString(5);
    }
}
