using System;
using System.IO;
using System.Threading.Tasks;
using SafeApp.Core;
using Xunit;

namespace SafeAppTests
{
    [Collection("Files Tests")]
    public class FilesTest : IDisposable
    {
        public FilesTest()
        {
            TestUtils.PrepareTestData();
        }

        public void Dispose()
        {
            TestUtils.RemoveTestData();
        }

        [Fact]
        public async Task FilesContainerCreateAndGetTest()
        {
            var session = await TestUtils.CreateTestApp();
            var (xorUrl, processedFiles, filesMap1) = await session.Files.FilesContainerCreateAsync(
                TestUtils.TestDataDir,
                null,
                true,
                false);
            await Validate.XorUrlAsync(xorUrl, DataType.PublishedSeqAppendOnlyData, ContentType.FilesContainer, 1100);
            Assert.NotEqual(default, processedFiles);

            // TODO: fix this; incorrect way of testing equality of this struct
            // Assert.NotEqual(default, processedFiles.Files.Find(q => q.FileName.Equals("index.html")));

            Assert.NotNull(filesMap1);

            var (version, filesMap2) = await session.Files.FilesContainerGetAsync(xorUrl);
            Assert.Equal<ulong>(0, version);
        }

        [Fact]
        public Task FilesContainerSyncTest()
            => RunSyncTest(dryRun: false);

        [Fact]
        public Task FilesContainerSyncDryRunTest()
            => RunSyncTest(dryRun: true);

        [Fact]
        public Task FilesContainerAddTest()
            => RunAddTest(dryRun: false);

        [Fact]
        public Task FilesContainerAddDryRunTest()
            => RunAddTest(dryRun: true);

        [Fact]
        public async Task FilesContainerAddFromRawTest()
        {
            var session = await TestUtils.CreateTestApp();
            var (xorUrl, processedFiles, filesMap) = await session.Files.FilesContainerCreateAsync(TestUtils.TestDataDir, null, false, false);
            var newFileName = $"{xorUrl}/hello.html";
            var (version, newProcessedFiles, newFilesMap) = await session.Files.FilesContainerAddFromRawAsync(TestUtils.GetRandomString(50).ToUtfBytes(), newFileName, false, false, false);

            ValidateFiles(1, version, processedFiles, newProcessedFiles, filesMap, newFilesMap);
        }

        [Fact]
        public async Task FilesPutAndGetPublishedImmutableTest()
        {
            var session = await TestUtils.CreateTestApp();
            var data = TestUtils.GetRandomString(20).ToUtfBytes();
            var xorUrl = await session.Files.FilesPutPublishedImmutableAsync(data, "text/plain");
            var newData = await session.Files.FilesGetPublishedImmutableAsync(xorUrl);
            Assert.Equal(data, newData);
        }

        async Task RunSyncTest(bool dryRun)
        {
            var session = await TestUtils.CreateTestApp();
            var (xorUrl, processedFiles, filesMap) = await session.Files.FilesContainerCreateAsync(TestUtils.TestDataDir, null, true, false);
            Directory.CreateDirectory($"{TestUtils.TestDataDir}/newDir");
            var testFilePath = Path.Combine($"{TestUtils.TestDataDir}/newDir", "hello.html");
            File.WriteAllText(testFilePath, TestUtils.GetRandomString(20));
            var (version, newProcessedFiles, newFilesMap) = await session.Files.FilesContainerSyncAsync(
                $"{TestUtils.TestDataDir}/newDir",
                xorUrl,
                recursive: true,
                delete: false,
                updateNrs: false,
                dryRun: dryRun);

            ValidateFiles(1, version, processedFiles, newProcessedFiles, filesMap, newFilesMap);
        }

        async Task RunAddTest(bool dryRun)
        {
            var session = await TestUtils.CreateTestApp();
            var (xorUrl, processedFiles, filesMap) = await session.Files.FilesContainerCreateAsync(TestUtils.TestDataDir, null, false, false);
            var testFilePath = Path.Combine(TestUtils.TestDataDir, "hello.html");
            File.WriteAllText(testFilePath, TestUtils.GetRandomString(20));
            var newFileName = $"{xorUrl}/hello.html";
            var (version, newProcessedFiles, newFilesMap) = await session.Files.FilesContainerAddAsync(
                $"{TestUtils.TestDataDir}/hello.html",
                newFileName,
                false,
                false,
                dryRun);

            ValidateFiles(1, version, processedFiles, newProcessedFiles, filesMap, newFilesMap);
        }

        void ValidateFiles(
            ulong expectedVersion,
            ulong actualVersion,
            ProcessedFiles originalProcessedFiles,
            ProcessedFiles newProcessedFiles,
            string originalFilesMap,
            string newFilesMap)
        {
            Assert.Equal(expectedVersion, actualVersion);

            // TODO: fix this; incorrect way of testing equality of this struct
            // Assert.NotEqual(default, newProcessedFiles.Files.Find(q => q.FileName.Equals("hello.html")));

            // TODO: fix this; incorrect way of testing equality of this struct
            Assert.NotEqual(originalProcessedFiles, newProcessedFiles);

            Assert.NotEqual(originalFilesMap, newFilesMap);
        }
    }
}
