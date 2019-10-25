using System;
using System.Threading.Tasks;
using SafeApp.API;
using SafeApp.Core;
using Xunit;

namespace SafeAppTests
{
    [Collection("XorUrl Encoder Tests")]
    public class XorUrlEncoderTest
    {
        [Fact]
        public async Task EncodeStringTestAsync()
        {
            var rnd = new Random();
            var xorName = new byte[32];
            rnd.NextBytes(xorName);
            var typeTag = 16000UL;
            var contentType = ContentType.Wallet;
            var dataType = DataType.UnpublishedImmutableData;
            var encodedString = await XorEncoder.EncodeAsync(
                xorName,
                typeTag,
                dataType,
                contentType,
                null,
                null,
                0,
                "base32z");
            Assert.NotEqual(string.Empty, encodedString);
            Assert.StartsWith("safe://", encodedString, StringComparison.Ordinal);

            var xorUrlEncoder = await XorEncoder.EncodeAsync(
                xorName,
                typeTag,
                dataType,
                contentType,
                null,
                null,
                0);

            Assert.Equal(xorName, xorUrlEncoder.XorName);
            Validate.Encoder(xorUrlEncoder, dataType, (ContentType)contentType, typeTag);

            var parsedEncoder = await XorEncoder.XorUrlEncoderFromUrl(encodedString);

            Assert.Equal(xorName, parsedEncoder.XorName);
            Validate.Encoder(parsedEncoder, dataType, contentType, typeTag);
            Assert.Equal(typeTag, parsedEncoder.TypeTag);
            Assert.Equal(contentType, parsedEncoder.ContentType);
            Assert.Equal<ulong>(0, parsedEncoder.ContentVersion);
        }
    }
}
