﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SafeApp.Core;
using SafeApp.MockAuthBindings;

namespace SafeApp.Tests.AuthConsole
{
    class Program
    {
        private static readonly Random _random = new Random();
        private static Authenticator _authenticator;

        static async Task Main(string[] args)
        {
            try
            {
                var locator = GenerateRandomString(10);
                var secret = GenerateRandomString(10);
                await CreateTestAccount(locator, secret);
                if (_authenticator == null)
                    throw new ApplicationException("Authenticator object null. Can't proceed further");

                var authRequestMsg = await GenerateRandomAuthReq();
                var authResponseMsg = await AuthenticateTestApp(authRequestMsg);

                var currentDirectory = Environment.CurrentDirectory;
                Console.WriteLine(args[0]);
                var fileSaveDirectory = args.Length == 1 ? args[0] : @"..\..\..\..";

                var fileSavePath = Path.Combine(fileSaveDirectory, "TestAuthResponse.txt");

                if (File.Exists(fileSavePath))
                    File.Delete(fileSavePath);

                File.WriteAllText(fileSavePath, authResponseMsg);
                Console.WriteLine($"Test auth response file stored at: {fileSavePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Failed to perform authentication using non-mock lib.");
            }
        }

        private static async Task<string> GenerateRandomAuthReq()
        {
            var authReq = new AuthReq
            {
                App = new AppExchangeInfo
                {
                    Id = "net.maidsafe.testApp",
                    Name = GenerateRandomString(5),
                    Scope = null,
                    Vendor = GenerateRandomString(5),
                },
                AppContainer = true,
                AppPermissionTransferCoins = true,
                AppPermissionGetBalance = true,
                AppPermissionPerformMutations = true,
                Containers = new List<ContainerPermissions>(),
            };
            var (_, reqMsg) = await Session.EncodeAuthReqAsync(authReq);
            return reqMsg;
        }

        private static async Task CreateTestAccount(string locator, string secret)
        {
            _authenticator = await Authenticator.CreateAccountAsync(locator, secret);
            Console.WriteLine("test account created");
        }

        private static async Task<string> AuthenticateTestApp(string authRequestMsg)
        {
            Console.WriteLine("authenticating test app");
            var ipcReq = await _authenticator.DecodeIpcMessageAsync(authRequestMsg);
            var authIpcReq = ipcReq as AuthIpcReq;
            return await _authenticator.EncodeAuthRespAsync(authIpcReq, true);
        }

        private static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
        }
    }
}