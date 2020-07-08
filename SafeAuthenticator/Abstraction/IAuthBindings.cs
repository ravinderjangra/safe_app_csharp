using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SafeApp.Core;

namespace SafeAuthenticator
{
    internal partial interface IAuthBindings
    {
        bool AuthIsMock();

        void LoginAsync(string passphrase, string password, Action<FfiResult, IntPtr, GCHandle> oCb);

        Task LogOutAsync(IntPtr app);

        Task<bool> IsLoggedInAsync(IntPtr app);

        void CreateAccountAsync(string secretKey, string passphrase, string password, Action<FfiResult, IntPtr, GCHandle> oCb);

        Task<string> AutheriseAppAsync(IntPtr app, string request, bool isGranted);

        Task RevokeAppAsync(IntPtr app, string appId);

        Task<List<AuthedApp>> AuthdAppAsync(IntPtr app);

        Task<string> AutheriseUnregisteredAppAsync(uint reqId, bool isGranted);

        Task<IpcReq> DecodeIpcMessage(IntPtr authPtr, string msg);

        Task<IpcReq> UnRegisteredDecodeIpcMsgAsync(string msg);
    }
}
