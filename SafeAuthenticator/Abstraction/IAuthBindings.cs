using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SafeAuthenticator
{
    internal partial interface IAuthBindings
    {
        Task<IntPtr> LogInAsync(string passphrase, string password);

        Task LogOutAsync(IntPtr app);

        Task<bool> IsLoggedInAsync(IntPtr app);

        Task<IntPtr> CreateAccAsync(string secretKey, string passphrase, string password);

        Task<string> AutheriseAppAsync(IntPtr app, string request, bool isGranted);

        Task RevokeAppAsync(IntPtr app, string appId);

        Task<List<AuthedApp>> AuthdAppAsync(IntPtr app);

        Task<string> AutheriseUnregisteredAppAsync(uint reqId, bool isGranted);
    }
}
