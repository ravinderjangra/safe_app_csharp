﻿using System;
using System.Threading.Tasks;
using SafeApp;
using SafeApp.API;
using SafeApp.Core;
using Xunit;

namespace SafeAppTests
{
    [Collection("Nrs Tests")]
    public class NrsTest : IDisposable
    {
        private const bool SetDefault = true;
        private const bool DirectLink = true;
        private const bool DryRun = false;

        public NrsTest()
        {
            TestUtils.PrepareTestData();
        }

        public void Dispose()
        {
            TestUtils.RemoveTestData();
        }

        [Fact]
        public async Task ParseUrlTest()
        {
            var session = await TestUtils.CreateTestApp();
            var (xorUrl, _) = await session.Keys.KeysCreatePreloadTestCoinsAsync("1");

            var xorUrlEncoder = await Nrs.ParseUrlAsync(xorUrl);

            // todo: verify that these are actually the expected values
            Assert.Equal(ContentType.Raw, xorUrlEncoder.ContentType);
            Assert.Equal<ulong>(0, xorUrlEncoder.ContentVersion);
            Assert.Equal(DataType.SafeKey, xorUrlEncoder.DataType);
            Assert.Equal<ulong>(1, xorUrlEncoder.EncodingVersion);
            Assert.Equal(string.Empty, xorUrlEncoder.Path);
            Assert.Equal("[]", xorUrlEncoder.SubNames);
            Assert.Equal<ulong>(0, xorUrlEncoder.TypeTag);
            Validate.XorName(xorUrlEncoder.XorName);
        }

        [Fact(Skip = "Needs to be fixed. Ignoring to test the Android and iOS libs on CI.")]
        public async Task ParseAndResolveUrlTest()
        {
            var session = await TestUtils.CreateTestApp();

            var xorUrl = await CreateFilesContainerAsync(session);

            var api = session.Nrs;
            var (_, _, nrsMapXorUrl) = await api.CreateNrsMapContainerAsync(
                TestUtils.GetRandomString(5),
                $"{xorUrl}?v=0",
                false,
                DryRun,
                SetDefault);
            var (xorUrlEncoder, resolvedFrom) = await api.ParseAndResolveUrlAsync(nrsMapXorUrl);

            Validate.Encoder(xorUrlEncoder, DataType.PublishedSeqAppendOnlyData, ContentType.FilesContainer, 1100);
            Validate.Encoder(resolvedFrom, DataType.PublishedSeqAppendOnlyData, ContentType.NrsMapContainer, 1500);
        }

        [Fact]
        public async Task CreateNrsMapContainerTest()
        {
            var session = await TestUtils.CreateTestApp();
            var name = TestUtils.GetRandomString(5);

            var link = await CreateFilesContainerAsync(session);

            var api = session.Nrs;
            var (nrsMapRaw, processedEntries, xorUrl) = await api.CreateNrsMapContainerAsync(
                name,
                $"{link}?v=0",
                false,
                DryRun,
                SetDefault);

            Assert.NotEqual(default, processedEntries);
            await Validate.RawNrsMapAsync(nrsMapRaw);
            await Validate.XorUrlAsync(xorUrl, DataType.PublishedSeqAppendOnlyData, ContentType.NrsMapContainer, 1500);
        }

        [Fact]
        public async Task AddToNrsMapContainerTest()
        {
            var session = await TestUtils.CreateTestApp();
            var name = TestUtils.GetRandomString(5);
            var xorUrlResult = await CreateNrsMapContainerXorUrlAsync(session, name);

            var link = await CreateFilesContainerAsync(session);

            var (nrsMapRaw, xorUrl, version) = await session.Nrs.AddToNrsMapContainerAsync(
                name,
                $"{link}?v=0",
                SetDefault,
                DirectLink,
                DryRun);

            Assert.Equal<ulong>(1, version);
            await Validate.RawNrsMapAsync(nrsMapRaw);
            await Validate.XorUrlAsync(xorUrl, DataType.PublishedSeqAppendOnlyData, ContentType.NrsMapContainer, 1500);
        }

        [Fact]
        public async Task RemoveFromNrsMapContainerTest()
        {
            var session = await TestUtils.CreateTestApp();
            var name = TestUtils.GetRandomString(5);
            var xorUrlResult = await CreateNrsMapContainerXorUrlAsync(session, name);

            var (nrsMapRaw, xorUrl, version) = await session.Nrs.RemoveFromNrsMapContainerAsync(name, DryRun);

            Assert.Equal<ulong>(1, version);
            Assert.Equal("{\"sub_names_map\":{},\"default\":\"NotSet\"}", nrsMapRaw);
            await Validate.XorUrlAsync(xorUrl, DataType.PublishedSeqAppendOnlyData, ContentType.NrsMapContainer, 1500);
        }

        [Fact]
        public async Task GetNrsMapContainerTest()
        {
            var session = await TestUtils.CreateTestApp();
            var name = TestUtils.GetRandomString(5);
            var xorUrl = await CreateNrsMapContainerXorUrlAsync(session, name);

            var api = session.Nrs;
            var (nrsMapRaw, version) = await api.GetNrsMapContainerAsync(xorUrl);

            Assert.Equal<ulong>(0, version);
            await Validate.RawNrsMapAsync(nrsMapRaw);
            await Validate.XorUrlAsync(xorUrl, DataType.PublishedSeqAppendOnlyData, ContentType.NrsMapContainer, 1500);
        }

        async Task<string> CreateFilesContainerAsync(Session session)
        {
            var (xorUrl, _, _) = await session.Files.FilesContainerCreateAsync(
                TestUtils.TestDataDir,
                null,
                true,
                false);

            return xorUrl;
        }

        async Task<string> CreateNrsMapContainerXorUrlAsync(Session session, string name)
        {
            var link = await CreateFilesContainerAsync(session);

            var api = session.Nrs;
            var (_, _, xorUrl) = await api.CreateNrsMapContainerAsync(
                name,
                $"{link}?v=0",
                DirectLink,
                DryRun,
                SetDefault);

            return xorUrl;
        }
    }
}
