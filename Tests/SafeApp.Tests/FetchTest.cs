using System;
using System.Threading.Tasks;
using SafeApp.Core;
using Xunit;

namespace SafeAppTests
{
    [Collection("Fetch Tests")]
    public class FetchTest : IDisposable
    {
        public FetchTest()
        {
            TestUtils.PrepareTestData();
        }

        public void Dispose()
        {
            TestUtils.RemoveTestData();
        }

#if __ANDROID__ || __IOS__
        [Fact(Skip = "Fetch and Inspect API fails on Android and iOS")]
#else
        [Fact]
#endif
        public async Task FetchDataTypesTest()
        {
            var session = await TestUtils.CreateTestApp();

            var (keyUrl, keys) = await session.Keys.KeysCreatePreloadTestCoinsAsync("10");
            ValidateFetchOrInspectDataTypes(await session.Fetch.FetchAsync(keyUrl), isFetch: true);

            var walletUrl = await session.Wallet.WalletCreateAsync();
            await session.Wallet.WalletInsertAsync(walletUrl, TestUtils.GetRandomString(5), true, keys.SK);
            ValidateFetchOrInspectDataTypes(await session.Fetch.FetchAsync(walletUrl), isFetch: true);

            var (filesXorUrl, processedFiles, _) = await session.Files.FilesContainerCreateAsync(
                TestUtils.TestDataDir,
                null,
                true,
                false);
            ValidateFetchOrInspectDataTypes(await session.Fetch.FetchAsync(filesXorUrl), isFetch: true);
            ValidateFetchOrInspectDataTypes(await session.Fetch.FetchAsync(processedFiles.Files[0].FileXorUrl), isFetch: true);

            var (_, _, nrsXorUrl) = await session.Nrs.CreateNrsMapContainerAsync(
                TestUtils.GetRandomString(5),
                $"{filesXorUrl}?v=0",
                false,
                false,
                true);
            ValidateFetchOrInspectDataTypes(await session.Fetch.FetchAsync(nrsXorUrl), isFetch: true, expectNrs: true);
        }

#if __ANDROID__ || __IOS__
        [Fact(Skip = "Fetch and Inspect API fails on Android and iOS")]
#else
        [Fact]
#endif
        public async Task InspectDataTypesTest()
        {
            var session = await TestUtils.CreateTestApp();
            var (keyUrl, keys) = await session.Keys.KeysCreatePreloadTestCoinsAsync("10");
            ValidateFetchOrInspectDataTypes(await session.Fetch.InspectAsync(keyUrl));

            var walletUrl = await session.Wallet.WalletCreateAsync();
            await session.Wallet.WalletInsertAsync(walletUrl, TestUtils.GetRandomString(5), true, keys.SK);
            ValidateFetchOrInspectDataTypes(await session.Fetch.InspectAsync(walletUrl));

            var (filesXorUrl, processedFiles, _) = await session.Files.FilesContainerCreateAsync(
                TestUtils.TestDataDir,
                null,
                true,
                false);
            ValidateFetchOrInspectDataTypes(await session.Fetch.InspectAsync(filesXorUrl));
            ValidateFetchOrInspectDataTypes(await session.Fetch.InspectAsync(processedFiles.Files[0].FileXorUrl));

            var (_, _, nrsXorUrl) = await session.Nrs.CreateNrsMapContainerAsync(
                TestUtils.GetRandomString(5),
                $"{filesXorUrl}?v=0",
                false,
                false,
                true);
            ValidateFetchOrInspectDataTypes(await session.Fetch.InspectAsync(nrsXorUrl), expectNrs: true);
        }

        internal static void ValidateFetchOrInspectDataTypes(ISafeData data, bool isFetch = false, bool expectNrs = false)
        {
            if (data != null)
            {
                switch (data)
                {
                    case SafeKey key:
                        Validate.XorName(key.XorName);
                        Validate.EnsureNullNrsContainerInfo(key.ResolvedFrom);
                        break;
                    case Wallet wallet:
                        Validate.XorName(wallet.XorName);
                        Validate.EnsureNullNrsContainerInfo(wallet.ResolvedFrom);
                        if (isFetch)
                            Assert.NotEmpty(wallet.Balances.WalletBalances);
                        else
                            Assert.Empty(wallet.Balances.WalletBalances);
                        break;
                    case FilesContainer filesContainer:
                        Validate.XorName(filesContainer.XorName);
                        if (expectNrs)
                            Validate.NrsContainerInfo(filesContainer.ResolvedFrom);
                        else
                            Validate.EnsureNullNrsContainerInfo(filesContainer.ResolvedFrom);
                        break;
                    case PublishedImmutableData immutableData:
                        Assert.NotNull(immutableData.Data);
                        Validate.XorName(immutableData.XorName);
                        if (isFetch)
                            Assert.NotEmpty(immutableData.Data);
                        else
                            Assert.Empty(immutableData.Data);
                        if (expectNrs)
                            Validate.NrsContainerInfo(immutableData.ResolvedFrom);
                        else
                            Validate.EnsureNullNrsContainerInfo(immutableData.ResolvedFrom);
                        break;
                    case SafeDataFetchFailed dataFetchOrInspectFailed:
                        Assert.NotNull(dataFetchOrInspectFailed.Description);
                        Assert.NotEqual(0, dataFetchOrInspectFailed.Code);
                        TestUtils.Fail(dataFetchOrInspectFailed.Description);
                        break;
                }
            }
            else
            {
                if (isFetch)
                    TestUtils.Fail("Fetch data type not available");
                else
                    TestUtils.Fail("Inspect data type not available");
            }
        }
    }
}
