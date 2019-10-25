﻿using System;
using System.Threading.Tasks;
using SafeApp.Core;
using Xunit;

namespace SafeAppTests
{
    [Collection("Wallet Tests")]
    public class WalletTest
    {
        private string _testWalletOne = TestUtils.GetRandomString(10);
        private string _testWalletTwo = TestUtils.GetRandomString(10);

        [Fact]
        public async Task CreateWalletTest()
        {
            var (api, _) = await GetKeysAndWalletAPIs();

            var wallet = await api.WalletCreateAsync();
            var balance = await api.WalletBalanceAsync(wallet);
            Validate.IsEqualAmount("0.0", balance);
        }

        [Fact]
        public async Task InsertAndBalanceTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            var walletXorUrl = await api.WalletCreateAsync();
            var keyPair_1_Balance = "123";
            var keyPair_2_Balance = "321";
            var expectedEndBalance = "444";
            var (_, keyPair1) = await keysApi.KeysCreatePreloadTestCoinsAsync(keyPair_1_Balance);
            var (_, keyPair2) = await keysApi.KeysCreatePreloadTestCoinsAsync(keyPair_2_Balance);
            await api.WalletInsertAsync(walletXorUrl, _testWalletOne, setDefault: true, keyPair1.SK);

            var currentBalance = await api.WalletBalanceAsync(walletXorUrl);
            Validate.IsEqualAmount(keyPair_1_Balance, currentBalance);

            await api.WalletInsertAsync(walletXorUrl, _testWalletTwo, setDefault: false, keyPair2.SK);
            currentBalance = await api.WalletBalanceAsync(walletXorUrl);
            Validate.IsEqualAmount(expectedEndBalance, currentBalance);
        }

        [Fact]
        public async Task InsertAndGetTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            var walletXorUrl = await api.WalletCreateAsync();
            var (xorUrl1, keyPair1) = await keysApi.KeysCreatePreloadTestCoinsAsync("123");
            var (xorUrl2, keyPair2) = await keysApi.KeysCreatePreloadTestCoinsAsync("321");

            await api.WalletInsertAsync(walletXorUrl, _testWalletOne, setDefault: true, keyPair1.SK);
            await api.WalletInsertAsync(walletXorUrl, _testWalletTwo, setDefault: false, keyPair2.SK);

            var walletBalances = await api.WalletGetAsync(walletXorUrl);
            Assert.True(walletBalances.WalletBalances.Find(q => q.WalletName.Equals(_testWalletOne)).IsDefault);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletOne).XorUrl, xorUrl1);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletOne).Sk, keyPair1.SK);

            Assert.False(walletBalances.WalletBalances.Find(q => q.WalletName.Equals(_testWalletTwo)).IsDefault);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletTwo).XorUrl, xorUrl2);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletTwo).Sk, keyPair2.SK);
        }

        [Fact]
        public async Task WalletInsertAndSetDefaultTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            var walletXorUrl = await api.WalletCreateAsync();
            var (xorUrl1, keyPair1) = await keysApi.KeysCreatePreloadTestCoinsAsync("123");
            var (xorUrl2, keyPair2) = await keysApi.KeysCreatePreloadTestCoinsAsync("321");

            await api.WalletInsertAsync(walletXorUrl, _testWalletOne, setDefault: true, keyPair1.SK);
            await api.WalletInsertAsync(walletXorUrl, _testWalletTwo, setDefault: true, keyPair2.SK);
            var walletBalances = await api.WalletGetAsync(walletXorUrl);

            Assert.False(walletBalances.WalletBalances.Find(q => q.WalletName.Equals(_testWalletOne)).IsDefault);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletOne).XorUrl, xorUrl1);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletOne).Sk, keyPair1.SK);

            Assert.True(walletBalances.WalletBalances.Find(q => q.WalletName.Equals(_testWalletTwo)).IsDefault);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletTwo).XorUrl, xorUrl2);
            Assert.Equal(FindWalletBalance(walletBalances, _testWalletTwo).Sk, keyPair2.SK);
        }

        WalletSpendableBalance FindWalletBalance(WalletSpendableBalances list, string name)
            => list.WalletBalances.Find(q => q.WalletName.Equals(name)).Balance;

        [Fact]
        public async Task TransferWithoutDefaultBalanceTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            // without default balance
            var fromWalletXorUrl = await api.WalletCreateAsync();

            // with default balance
            var toWalletXorUrl = await api.WalletCreateAsync();
            var (_, keyPair) = await keysApi.KeysCreatePreloadTestCoinsAsync("123");
            await api.WalletInsertAsync(toWalletXorUrl, "TestBalance", setDefault: true, keyPair.SK);

            await AssertThrowsAsync(-203, () => api.WalletTransferAsync(fromWalletXorUrl, toWalletXorUrl, "123", 0));
            await AssertThrowsAsync(-203, () => api.WalletTransferAsync(toWalletXorUrl, fromWalletXorUrl, "123", 0));
        }

        [Fact]
        public async Task TransferFromZeroBalanceTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            var fromWalletXorUrl = await api.WalletCreateAsync();
            var (_, keyPair1) = await keysApi.KeysCreatePreloadTestCoinsAsync("0.0");
            await api.WalletInsertAsync(fromWalletXorUrl, "TestBalance", setDefault: true, keyPair1.SK);

            var (toXorUrl, _) = await keysApi.KeysCreatePreloadTestCoinsAsync("0.5");

            await AssertThrowsAsync(-301, () => api.WalletTransferAsync(fromWalletXorUrl, toXorUrl, "0.5", 0));

            var toWalletXorUrl = await api.WalletCreateAsync();
            var (xorurl2, keypair2) = await keysApi.KeysCreatePreloadTestCoinsAsync("0.5");
            await api.WalletInsertAsync(toWalletXorUrl, "NewTestBalance", setDefault: true, keypair2.SK);

            await AssertThrowsAsync(-300, () => api.WalletTransferAsync(fromWalletXorUrl, toWalletXorUrl, "0", 0));
        }

        [Fact]
        public async Task TransferDifferentAmountsTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            var fromWalletXorUrl = await api.WalletCreateAsync();
            var (_, keyPair1) = await keysApi.KeysCreatePreloadTestCoinsAsync("123.321");
            await api.WalletInsertAsync(fromWalletXorUrl, _testWalletOne, setDefault: true, keyPair1.SK);

            var (toXorUrl, keypair2) = await keysApi.KeysCreatePreloadTestCoinsAsync("0.5");
            await api.WalletInsertAsync(fromWalletXorUrl, "TestBalance2", setDefault: false, keypair2.SK);

            // transfering amount more than current balance
            await AssertThrowsAsync(-301, () => api.WalletTransferAsync(fromWalletXorUrl, toXorUrl, "321.123", 0));

            // transfering invalid amount
            await AssertThrowsAsync(-300, () => api.WalletTransferAsync(fromWalletXorUrl, toXorUrl, "0.dgnda", 0));

            // valid transfer
            await api.WalletTransferAsync(fromWalletXorUrl, toXorUrl, "10", 0);
        }

        [Fact]
        public async Task TransferToSafeKeyTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            var fromWalletXorUrl = await api.WalletCreateAsync();
            var (_, keyPair1) = await keysApi.KeysCreatePreloadTestCoinsAsync("123.321");
            await api.WalletInsertAsync(fromWalletXorUrl, "TestBalance", setDefault: true, keyPair1.SK);

            var (xorUrl2, _) = await keysApi.KeysCreatePreloadTestCoinsAsync("10.0");

            // transfer from wallet to key
            await api.WalletTransferAsync(fromWalletXorUrl, xorUrl2, "100.001", 0);
        }

        [Fact]
        public async Task TransferFromSafeKeyTest()
        {
            var (api, keysApi) = await GetKeysAndWalletAPIs();

            var (safekeyXorUrl1, _) = await keysApi.KeysCreatePreloadTestCoinsAsync("10");
            var (safekeyXorUrl2, _) = await keysApi.KeysCreatePreloadTestCoinsAsync("0");

            await AssertThrowsAsync(-207, () => api.WalletTransferAsync(safekeyXorUrl1, safekeyXorUrl2, "5", 0));
        }

        [Fact]
        public async Task TransferFromUnownedWalletTest()
        {
            var (api1, keysApi1) = await GetKeysAndWalletAPIs();

            var account1WalletXORURL = await api1.WalletCreateAsync();
            var (keyXorUrl1, keyPair1) = await keysApi1.KeysCreatePreloadTestCoinsAsync("123.321");
            await api1.WalletInsertAsync(account1WalletXORURL, "TestBalance", setDefault: true, keyPair1.SK);

            var (api2, keysApi2) = await GetKeysAndWalletAPIs();
            var (keyXorUrl, keyPair2) = await keysApi2.KeysCreatePreloadTestCoinsAsync("123.321");

            await AssertThrowsAsync(-102, () => api2.WalletTransferAsync(account1WalletXORURL, keyXorUrl, "5", 0));
        }

        async Task<(SafeApp.API.Wallet, SafeApp.API.Keys)> GetKeysAndWalletAPIs()
        {
            var session = await TestUtils.CreateTestApp();
            return (session.Wallet, session.Keys);
        }

        async Task AssertThrowsAsync(int errorCode, Func<Task> func)
        {
            var ex = await Assert.ThrowsAsync<FfiException>(func);
            Assert.Equal(errorCode, ex.ErrorCode);
        }
    }
}
