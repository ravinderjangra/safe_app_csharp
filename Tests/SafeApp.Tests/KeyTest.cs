using System.Threading.Tasks;
using NUnit.Framework;

namespace SafeApp.Tests
{
    [TestFixture]
    internal class KeyTest
    {
        private const string _preloadInitialAmount = "2";
        private const string _transferAmount = "1";
        private const string _preloadAmount = "1";
        private API.Keys _api;

        [SetUp]
        public async Task Setup()
        {
            var session = await TestUtils.CreateTestApp();
            _api = session.Keys;
        }

        [Test]
        public async Task GenerateKeyPairTest()
        {
            var keyPair = await _api.GenerateKeyPairAsync();
            Validate.TransientKeyPair(keyPair);
        }

        [Test]
        public async Task KeysCreatePreloadTestCoinsTest()
        {
            var (xorUrl, keyPair) = await Session.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            await Validate.PersistedKeyPair(xorUrl, keyPair, _api);
        }

        [Test]
        public async Task CreateKeysFromTransientTest()
        {
            var (_, keyPairSender) = await Session.KeysCreatePreloadTestCoinsAsync(_preloadInitialAmount);

            // transient keys, not persisted on the network
            var keyPairRecipient = await _api.GenerateKeyPairAsync();

            // we expect keyPairRecipient to persisted with
            // this call, and newKeyPair to be empty.
            var (xorUrl, newKeyPair) = await _api.CreateKeysAsync(
                keyPairSender.SK,
                _transferAmount,
                keyPairRecipient.PK);

            Assert.IsEmpty(newKeyPair.SK);
            Assert.IsNotNull(newKeyPair.PK);
            Assert.AreEqual(keyPairRecipient.PK, newKeyPair.PK);

            await Validate.PersistedKeyPair(xorUrl, keyPairRecipient, _api);

            var senderBalance = await _api.KeysBalanceFromSkAsync(keyPairSender.SK);
            Validate.IsEqualAmount("0.999999999", senderBalance);

            var recipientBalance = await _api.KeysBalanceFromSkAsync(keyPairRecipient.SK);
            Validate.IsEqualAmount(_transferAmount, recipientBalance);
        }

        [Test]
        public async Task CreateKeysTest()
        {
            var (_, keyPairSender) = await Session.KeysCreatePreloadTestCoinsAsync(_preloadInitialAmount);

            var (xorUrl, newKeyPair) = await _api.CreateKeysAsync(
                keyPairSender.SK,
                _transferAmount,
                null);

            await Validate.PersistedKeyPair(xorUrl, newKeyPair, _api);

            var senderBalance = await _api.KeysBalanceFromSkAsync(newKeyPair.SK);
            Validate.IsEqualAmount(_transferAmount, senderBalance);

            var recipientBalance = await _api.KeysBalanceFromSkAsync(newKeyPair.SK);
            Validate.IsEqualAmount(_transferAmount, recipientBalance);
        }

        [Test]
        public async Task KeysBalanceFromSkTest()
        {
            var (xorurl, keyPair) = await Session.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            var balance = await _api.KeysBalanceFromSkAsync(keyPair.SK);
            Validate.IsEqualAmount(_preloadAmount, balance);
        }

        [Test]
        public async Task KeysBalanceFromUrlTest()
        {
            var (xorurl, keyPair) = await Session.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            var balance = await _api.KeysBalanceFromUrlAsync(xorurl, keyPair.SK);
            Validate.IsEqualAmount(_preloadAmount, balance);
        }

        [Test]
        public async Task ValidateSkForUrlTest()
        {
            var (xorurl, keyPair) = await Session.KeysCreatePreloadTestCoinsAsync(_preloadAmount);
            var publicKey = await _api.ValidateSkForUrlAsync(keyPair.SK, xorurl);
            Assert.AreEqual(keyPair.PK, publicKey);
        }

        [Test]
        public async Task KeysTransferTest()
        {
            var initialRecipientBalance = "0.1";
            var expectedRecipientEndBalance = "1.1";
            var expectedSenderEndBalance = "0";

            var txId = 1234UL;

            var (senderUrl, keyPairSender) = await Session.KeysCreatePreloadTestCoinsAsync(_transferAmount);
            var (recipientUrl, keyPairRecipient) = await Session.KeysCreatePreloadTestCoinsAsync(initialRecipientBalance);
            var resultTxId = await _api.KeysTransferAsync(_transferAmount, keyPairSender.SK, recipientUrl, txId);

            Assert.AreEqual(txId, resultTxId);

            var senderBalance = await _api.KeysBalanceFromUrlAsync(senderUrl, keyPairSender.SK);
            Validate.IsEqualAmount(expectedSenderEndBalance, senderBalance);

            var recipientBalance = await _api.KeysBalanceFromUrlAsync(recipientUrl, keyPairRecipient.SK);
            Validate.IsEqualAmount(expectedRecipientEndBalance, recipientBalance);
        }
    }
}
