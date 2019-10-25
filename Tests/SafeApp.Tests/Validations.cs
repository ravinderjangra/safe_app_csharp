using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using SafeApp.API;
using SafeApp.Core;
using Xunit;

namespace SafeAppTests
{
    class Validate
    {
        public static void XorName(byte[] xorName)
        {
            Assert.NotNull(xorName);
            Assert.Equal(32, xorName.Length);
            Assert.False(Enumerable.SequenceEqual(new byte[32], xorName));
        }

        public static void NrsContainerInfo(NrsMapContainerInfo info)
        {
            Assert.NotEqual(DataType.SafeKey, info.DataType);
            Assert.NotNull(info.NrsMap);
            Assert.NotNull(info.PublicName);
            Assert.NotEqual<ulong>(0, info.TypeTag);
            Assert.Equal<ulong>(0, info.Version);
            Assert.NotNull(info.XorUrl);
            Validate.XorName(info.XorName);
        }

        public static void EnsureNullNrsContainerInfo(NrsMapContainerInfo info)
        {
            Assert.Equal(DataType.SafeKey, info.DataType); // since 0 is actually a data type
            Assert.Null(info.NrsMap);
            Assert.Null(info.PublicName);
            Assert.Equal<ulong>(0, info.TypeTag);
            Assert.Equal<ulong>(0, info.Version); // since v 0 is actually the first version
            Assert.Null(info.XorUrl);
            Assert.True(Enumerable.SequenceEqual(new byte[32], info.XorName));
        }

        public static async Task XorUrlAsync(string xorUrl, DataType expectedDataType, ContentType expectedContentType, ulong expectedTypeTag)
        {
            var encoder = await XorEncoder.XorUrlEncoderFromUrl(xorUrl);
            Encoder(encoder, expectedDataType, expectedContentType, expectedTypeTag);
        }

        public static void Encoder(XorUrlEncoder encoder, DataType expectedDataType, ContentType expectedContentType, ulong expectedTypeTag)
        {
            Assert.Equal(expectedContentType, encoder.ContentType);
            Assert.Equal<ulong>(0, encoder.ContentVersion);
            Assert.Equal(expectedDataType, encoder.DataType);
            Assert.Equal<ulong>(1, encoder.EncodingVersion);

            // todo: these need to be validated once they contain the correct values

            /**
            Assert.AreEqual(string.Empty, encoder.Path);
            Assert.AreEqual(string.Empty, encoder.SubNames);
            **/
            Assert.Equal(expectedTypeTag, encoder.TypeTag);
            Validate.XorName(encoder.XorName);
        }

        public static async Task RawNrsMapAsync(string nrsMapRaw)
        {
            Assert.NotNull(nrsMapRaw);
            var nrsMap = Serialization.Deserialize<NrsMap>(nrsMapRaw);

            Assert.Null(nrsMap.SubNamesMap);
            /**
            foreach (var entry in nrsMap.SubNamesMap.Values)
            {
                Assert.IsNotNull(entry.SubName);
                Assert.IsNotNull(entry.SubNameRdf);
            }
            **/
            Assert.NotNull(nrsMap.Default);
            Assert.Single(nrsMap.Default);
            foreach (var rdf in nrsMap.Default.Values)
            {
                Assert.NotEqual(default(DateTime), rdf.Created);
                Assert.NotEqual(default(DateTime), rdf.Modified);
                Assert.NotNull(rdf.Link);
                await Validate.XorUrlAsync(rdf.Link, DataType.PublishedSeqAppendOnlyData, ContentType.FilesContainer, 1100);
            }
        }

        public static void TransientKeyPair(BlsKeyPair keyPair)
        {
            Assert.NotNull(keyPair.PK);
            Assert.NotNull(keyPair.SK);
            Assert.NotEmpty(keyPair.PK);
            Assert.NotEmpty(keyPair.SK);
            Assert.NotSame(keyPair.PK, keyPair.SK);
        }

        public static async Task PersistedKeyPair(string xorUrl, BlsKeyPair keyPair, Keys api)
        {
            Validate.TransientKeyPair(keyPair);
            await Validate.XorUrlAsync(xorUrl, DataType.SafeKey, ContentType.Raw, 0);
            var publicKey = await api.ValidateSkForUrlAsync(keyPair.SK, xorUrl);
            Assert.Equal(keyPair.PK, publicKey);
        }

        public static void IsEqualAmount(string expected, string actual)
        {
            Assert.Equal(
                decimal.Parse(expected, CultureInfo.InvariantCulture),
                decimal.Parse(actual, CultureInfo.InvariantCulture));
        }
    }
}
