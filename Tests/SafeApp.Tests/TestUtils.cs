﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SafeApp;
using SafeApp.Core;
using SafeApp.MockAuthBindings;
using Xunit;

namespace SafeAppTests
{
    public static class TestUtils
    {
        public static readonly Random Random = new Random();

        private static async Task<string> AuthenticateAuthRequest(
            Authenticator authenticator,
            string ipcMsg,
            bool allow)
        {
            var ipcReq = await authenticator.DecodeIpcMessageAsync(ipcMsg);
            Assert.IsType<AuthReq>(ipcReq);
            var response = await authenticator.EncodeAuthRespAsync(ipcReq as AuthIpcReq, allow);
            authenticator.Dispose();
            return response;
        }

        public static async Task<string> AuthenticateAuthRequest(string ipcMsg, bool allow)
        {
            var authenticator = await Authenticator.CreateAccountAsync(
                GetRandomString(10),
                GetRandomString(10));
            return await AuthenticateAuthRequest(authenticator, ipcMsg, allow);
        }

        public static async Task<string> AuthenticateAuthRequest(
            string locator,
            string secret,
            string ipcMsg,
            bool allow)
        {
            var authenticator = await Authenticator.LoginAsync(locator, secret);
            return await AuthenticateAuthRequest(authenticator, ipcMsg, allow);
        }

        public static async Task<string> AuthenticateUnregisteredRequest(string ipcMsg)
        {
            var ipcReq = await Authenticator.UnRegisteredDecodeIpcMsgAsync(ipcMsg);
            Assert.IsType<UnregisteredIpcReq>(ipcReq);
            var response = await Authenticator.EncodeUnregisteredRespAsync(((UnregisteredIpcReq)ipcReq).ReqId, true);
            return response;
        }

        public static Task<Session> CreateTestApp()
        {
            var locator = GetRandomString(10);
            var secret = GetRandomString(10);
            var authReq = new AuthReq
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
            return CreateTestApp(locator, secret, authReq);
        }

        public static Task<Session> CreateTestApp(AuthReq authReq)
        {
            var locator = GetRandomString(10);
            var secret = GetRandomString(10);
            return CreateTestApp(locator, secret, authReq);
        }

        public static async Task<Session> CreateTestApp(string locator, string secret, AuthReq authReq)
        {
            var authenticator = await Authenticator.CreateAccountAsync(locator, secret);
            var (_, reqMsg) = await Session.EncodeAuthReqAsync(authReq);
            var ipcReq = await authenticator.DecodeIpcMessageAsync(reqMsg);
            Assert.IsType<AuthIpcReq>(ipcReq);
            var authIpcReq = ipcReq as AuthIpcReq;
            var resMsg = await authenticator.EncodeAuthRespAsync(authIpcReq, true);
            return await Session.AppConnectAsync(authReq.App.Id, resMsg);
        }

        public static string GetRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
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

        public static void Fail(string message)
        => throw new Xunit.Sdk.XunitException(message);
    }
}
