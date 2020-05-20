using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SafeApp.Core;

namespace SafeAuthenticator
{
    /// <summary>
    /// The Authenticator contains all authentication related functionality for the mock network.
    /// </summary>
    public class Authenticator : IDisposable
    {
        private static readonly IAuthBindings NativeBindings = AuthResolver.Current;

        /// <summary>
        /// Event triggered if session is disconnected from network.
        /// </summary>
        private IntPtr _authPtr;

        /// <summary>
        /// Returns true if the native library was compiled with mock-routing feature.
        /// </summary>
        /// <returns>True if compiled with mock-routing feature otherwise false.</returns>
        public static bool IsMockBuild()
        {
            return NativeBindings.AuthIsMock();
        }

        /// <summary>
        /// Create new Account with a provided set of keys.
        /// </summary>
        /// <param name="secretKey">SecretKey.</param>
        /// <param name="passphrase">Account passphrase.</param>
        /// <param name="password">Account password.</param>
        /// <returns>New Authenticator instance.</returns>
        public static Task<Authenticator> CreateAccountAsync(string secretKey, string passphrase, string password)
        {
            return Task.Run(
              () =>
              {
                  var authenticator = new Authenticator();
                  var tcs = new TaskCompletionSource<Authenticator>(TaskCreationOptions.RunContinuationsAsynchronously);
                  Action<FfiResult, IntPtr, GCHandle> cb = (result, ptr, disconnectHandle) =>
                  {
                      if (result.ErrorCode != 0)
                      {
                          tcs.SetException(result.ToException());
                          return;
                      }

                      authenticator.Init(ptr, disconnectHandle);
                      tcs.SetResult(authenticator);
                  };
                  NativeBindings.CreateAccountAsync(secretKey, passphrase, password, cb);
                  return tcs.Task;
              });
        }

        /// <summary>
        /// Encode unregistered client authentication response.
        /// </summary>
        /// <param name="reqId">Request Id.</param>
        /// <param name="allow">Pass true to allow unregistered client authentication request. False to deny.</param>
        /// <returns>Encoded unregistered client authentication response string.</returns>
        public static Task<string> AutheriseUnregisteredAppAsync(uint reqId, bool allow)
        {
            return NativeBindings.AutheriseUnregisteredAppAsync(reqId, allow);
        }

        private Authenticator()
        {
            _authPtr = IntPtr.Zero;
        }

        /// <summary>
        /// Public implementation of Dispose pattern callable by developers.
        /// </summary>
        public void Dispose()
        {
            FreeAuth();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get the list of apps which was granted access by the user.
        /// </summary>
        /// <returns>List of registered apps.</returns>
        public Task<List<AuthedApp>> AuthRegisteredAppsAsync()
        {
            return NativeBindings.AuthdAppAsync(_authPtr);
        }

        /// <summary>
        ///  Revoke app access.
        /// </summary>
        /// <param name="appId">App Id.</param>
        /// <returns></returns>
        public Task AuthRevokeAppAsync(string appId)
        {
            return NativeBindings.RevokeAppAsync(_authPtr, appId);
        }

        /// <summary>
        /// Allow or deny an AuthIpcReq.
        /// </summary>
        /// <param name="encodedRequest">Encoded authentication IPC request.</param>
        /// <param name="allow">Pass true to accept the authentication request and false to deny.</param>
        /// <returns>Encoded AuthIpcResponse string.</returns>
        public Task<string> AutheriseAppAsync(string encodedRequest, bool allow)
        {
            return NativeBindings.AutheriseAppAsync(_authPtr, encodedRequest, allow);
        }

        /// <summary>
        /// Class destructor.
        /// </summary>
        ~Authenticator()
        {
            FreeAuth();
        }

        private void FreeAuth()
        {
            if (_authPtr == IntPtr.Zero)
            {
                return;
            }

            _authPtr = IntPtr.Zero;
        }

        private void Init(IntPtr authPtr, GCHandle disconnectedHandle)
        {
            _authPtr = authPtr;
        }

        /// <summary>
        /// Log-in to a registered account.
        /// </summary>
        /// <param name="passphrase">Account passphrase.</param>
        /// <param name="password">Account password.</param>
        /// <returns>New authenticator instance.</returns>
        public static Task<Authenticator> LoginAsync(string passphrase, string password)
        {
            return Task.Run(
              () =>
              {
                  var authenticator = new Authenticator();
                  var tcs = new TaskCompletionSource<Authenticator>(TaskCreationOptions.RunContinuationsAsynchronously);
                  Action<FfiResult, IntPtr, GCHandle> cb = (result, ptr, disconnectHandle) =>
                  {
                      if (result.ErrorCode != 0)
                      {
                          tcs.SetException(result.ToException());
                          return;
                      }

                      authenticator.Init(ptr, disconnectHandle);
                      tcs.SetResult(authenticator);
                  };
                  NativeBindings.LoginAsync(passphrase, password, cb);
                  return tcs.Task;
              });
        }
    }
}
