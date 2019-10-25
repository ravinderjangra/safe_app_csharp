using System.Threading.Tasks;
using Xunit;

namespace SafeAppTests
{
    [Collection("Keys Tests")]
    public class KeyTest
    {
        private const string _preloadInitialAmount = "2";
        private const string _transferAmount = "1";
        private const string _preloadAmount = "1";

        [Fact]
        public async Task GenerateKeyPairTest()
        {
            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var keyPair = await api.GenerateKeyPairAsync();
            Validate.TransientKeyPair(keyPair);
        }

        [Fact]
        public async Task KeysCreatePreloadTestCoinsTest()
        {
            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var (xorUrl, keyPair) = await api.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            await Validate.PersistedKeyPair(xorUrl, keyPair, api);
        }

        [Fact]
        public async Task CreateKeysFromTransientTest()
        {
            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var (_, keyPairSender) = await api.KeysCreatePreloadTestCoinsAsync(_preloadInitialAmount);

            // transient keys, not persisted on the network
            var keyPairRecipient = await api.GenerateKeyPairAsync();

            // we expect keyPairRecipient to persisted with
            // this call, and newKeyPair to be empty.
            var (xorUrl, newKeyPair) = await api.CreateKeysAsync(
                keyPairSender.SK,
                _transferAmount,
                keyPairRecipient.PK);

            Assert.Equal(string.Empty, newKeyPair.SK);
            Assert.NotNull(newKeyPair.PK);
            Assert.Equal(keyPairRecipient.PK, newKeyPair.PK);

            await Validate.PersistedKeyPair(xorUrl, keyPairRecipient, api);

            var senderBalance = await api.KeysBalanceFromSkAsync(keyPairSender.SK);
            Validate.IsEqualAmount("0.999999999", senderBalance);

            var recipientBalance = await api.KeysBalanceFromSkAsync(keyPairRecipient.SK);
            Validate.IsEqualAmount(_transferAmount, recipientBalance);
        }

        [Fact]
        public async Task CreateKeysTest()
        {
            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var (_, keyPairSender) = await api.KeysCreatePreloadTestCoinsAsync(_preloadInitialAmount);

            var (xorUrl, newKeyPair) = await api.CreateKeysAsync(
                keyPairSender.SK,
                _transferAmount,
                null);

            await Validate.PersistedKeyPair(xorUrl, newKeyPair, api);

            var senderBalance = await api.KeysBalanceFromSkAsync(newKeyPair.SK);
            Validate.IsEqualAmount(_transferAmount, senderBalance);

            var recipientBalance = await api.KeysBalanceFromSkAsync(newKeyPair.SK);
            Validate.IsEqualAmount(_transferAmount, recipientBalance);
        }

        [Fact]
        public async Task KeysBalanceFromSkTest()
        {
            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var (xorurl, keyPair) = await api.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            var balance = await api.KeysBalanceFromSkAsync(keyPair.SK);
            Validate.IsEqualAmount(_preloadAmount, balance);
        }

        [Fact]
        public async Task KeysBalanceFromUrlTest()
        {
            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var (xorurl, keyPair) = await api.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            var balance = await api.KeysBalanceFromUrlAsync(xorurl, keyPair.SK);
            Validate.IsEqualAmount(_preloadAmount, balance);
        }

        [Fact]
        public async Task ValidateSkForUrlTest()
        {
            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var (xorurl, keyPair) = await api.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            var publicKey = await api.ValidateSkForUrlAsync(keyPair.SK, xorurl);
            Assert.Equal(keyPair.PK, publicKey);
        }

        [Fact]
        public async Task KeysTransferTest()
        {
            var initialRecipientBalance = "0.1";
            var expectedRecipientEndBalance = "1.1";
            var expectedSenderEndBalance = "0";

            var txId = 1234UL;

            var session = await TestUtils.CreateTestApp();
            var api = session.Keys;
            var (senderUrl, keyPairSender) = await api.KeysCreatePreloadTestCoinsAsync(_transferAmount);
            var (recipientUrl, keyPairRecipient) = await api.KeysCreatePreloadTestCoinsAsync(initialRecipientBalance);
            var resultTxId = await api.KeysTransferAsync(_transferAmount, keyPairSender.SK, recipientUrl, txId);

            Assert.Equal(txId, resultTxId);

            var senderBalance = await api.KeysBalanceFromUrlAsync(senderUrl, keyPairSender.SK);
            Validate.IsEqualAmount(expectedSenderEndBalance, senderBalance);

            var recipientBalance = await api.KeysBalanceFromUrlAsync(recipientUrl, keyPairRecipient.SK);
            Validate.IsEqualAmount(expectedRecipientEndBalance, recipientBalance);
        }
    }
}
