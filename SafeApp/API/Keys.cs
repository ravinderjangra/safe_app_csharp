using System.Threading.Tasks;
using SafeApp.AppBindings;
using SafeApp.Core;

namespace SafeApp.API
{
    /// <summary>
    /// Keys API.
    /// </summary>
    public class Keys
    {
        static readonly IAppBindings AppBindings = AppResolver.Current;
        readonly SafeAppPtr _appPtr;

        /// <summary>
        /// Initialise a Keys object for the Session instance.
        /// The app pointer is required to perform network operations.
        /// </summary>
        /// <param name="appPtr">SafeApp pointer.</param>
        internal Keys(SafeAppPtr appPtr)
            => _appPtr = appPtr;

        /// <summary>
        /// Generate a key pair without creating and/or storing a SafeKey on the network.
        /// </summary>
        /// <returns>new BlsKeyPair instance.</returns>
        public Task<BlsKeyPair> GenerateKeyPairAsync()
            => AppBindings.GenerateKeyPairAsync(_appPtr);

        /// <summary>
        /// Create a SafeKey on the network and returns its XOR-URL.
        /// Optionally pre-loads it with an amount.
        /// Also returns new key pair if a recipient public key was not provided.
        /// </summary>
        /// <param name="from">Secret key of source balance.</param>
        /// <param name="preloadAmount">Optional test coins amount to add.</param>
        /// <param name="pk">Public key of recipient if provided.</param>
        /// <returns>XOR url of the balance, and a new key pair if no public key was provided.</returns>
        public Task<(string, BlsKeyPair)> CreateKeysAsync(
            string from,
            string preloadAmount,
            string pk)
            => AppBindings.CreateKeysAsync(_appPtr, from, preloadAmount, pk);

        /// <summary>
        /// Check SafeKey's balance from the network from a given SecretKey string.
        /// </summary>
        /// <param name="sk">The secret key to check the coins balance.</param>
        /// <returns>The available test coins balance.</returns>
        public Task<string> KeysBalanceFromSkAsync(string sk)
            => AppBindings.KeysBalanceFromSkAsync(_appPtr, sk);

        /// <summary>
        /// Check SafeKey's balance from the network from a given XOR/NRS-URL and secret key string.
        /// The difference between this and 'keys_balance_from_sk' function is that this will additionally
        /// check that the XOR/NRS-URL corresponds to the public key derived from the provided secret key.
        /// </summary>
        /// <param name="url">The SafeKey's XorUrl.</param>
        /// <param name="sk">The secret key required to check the balance.</param>
        /// <returns>The available test coins balance.</returns>
        public Task<string> KeysBalanceFromUrlAsync(string url, string sk)
            => AppBindings.KeysBalanceFromUrlAsync(_appPtr, url, sk);

        /// <summary>
        /// Check that the XOR/NRS-URL corresponds to the public key derived from the provided secret key.
        /// </summary>
        /// <param name="url">The SafeKey's XorUrl.</param>
        /// <param name="sk">The secret key required to check the balance.</param>
        /// <returns>The public key derived from the secret key.</returns>
        public Task<string> ValidateSkForUrlAsync(string sk, string url)
            => AppBindings.ValidateSkForUrlAsync(_appPtr, sk, url);

        /// <summary>
        /// Transfers safe coins from one SafeKey to another, or to a Wallet.
        /// </summary>
        /// <param name="amount">The test coin's amount to transfer.</param>
        /// <param name="fromSk">The sender's secret key.</param>
        /// <param name="toUrl">The recipient's XorUrl.</param>
        /// <param name="txId">An optional transaction id. A random will be returned if none specified.</param>
        /// <returns>Transaction Id.</returns>
        public Task<ulong> KeysTransferAsync(string amount, string fromSk, string toUrl, ulong txId)
            => AppBindings.KeysTransferAsync(_appPtr, amount, fromSk, toUrl, txId);
    }
}
