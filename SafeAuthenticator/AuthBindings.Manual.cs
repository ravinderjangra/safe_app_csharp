using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SafeApp.Core;

#if __IOS__
using ObjCRuntime;
#endif

namespace SafeAuthenticator
{
    internal partial class AuthBindings : IAuthBindings
    {
        public Task<IpcReq> DecodeIpcMessage(IntPtr authPtr, string msg)
        {
            var (task, userData) = BindingUtils.PrepareTask<IpcReq>();
            DecodeReqNative(
              authPtr,
              msg,
              userData,
              DelegateOnDecodeIpcReqAuthCb,
              DelegateOnDecodeIpcReqContainersCb,
              DelegateOnDecodeIpcReqUnregisteredCb,
              DelegateOnFfiResultCb);
            return task;
        }

        public void LoginAsync(string passphrase, string password, Action<FfiResult, IntPtr, GCHandle> cb)
        {
            var userData = BindingUtils.ToHandlePtr(cb);
            LogInNative(passphrase, password, userData, DelegateOnFfiResultSafeAuthenticatorCb);
        }

        public void CreateAccountAsync(string secretKey, string passphrase, string password, Action<FfiResult, IntPtr, GCHandle> cb)
        {
            var userData = BindingUtils.ToHandlePtr(cb);
            CreateAccNative(secretKey, passphrase, password, userData, DelegateOnFfiResultSafeAuthenticatorCb);
        }

        public Task<IpcReq> UnRegisteredDecodeIpcMsgAsync(string msg)
        {
            var (task, userData) = BindingUtils.PrepareTask<IpcReq>();
            DecodeAuthUnregisteredReqNative(msg, userData, DelegateOnDecodeIpcReqUnregisteredCb, DelegateOnFfiResultCb);
            return task;
        }

        private delegate void FfiResultSafeAuthenticatorCb(IntPtr userData, IntPtr result, IntPtr auth);

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultSafeAuthenticatorCb))]
#endif
        private static void OnFfiResultSafeAuthenticatorCb(IntPtr userData, IntPtr result, IntPtr auth)
        {
            var action = BindingUtils.FromHandlePtr<Action<FfiResult, IntPtr, GCHandle>>(userData, false);
            action(Marshal.PtrToStructure<FfiResult>(result), auth, GCHandle.FromIntPtr(userData));
        }

        private static readonly FfiResultSafeAuthenticatorCb DelegateOnFfiResultSafeAuthenticatorCb = OnFfiResultSafeAuthenticatorCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(NoneCb))]
#endif
        private static void OnAuthenticatorDisconnectCb(IntPtr userData)
        {
            var (action, _) = BindingUtils.FromHandlePtr<(Action, Action<FfiResult, IntPtr, GCHandle>)>(userData, false);

            action();
        }

        private static readonly NoneCb DelegateOnAuthenticatorDisconnectCb = OnAuthenticatorDisconnectCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(UIntAuthReqCb))]
#endif
        private static void OnDecodeIpcReqAuthCb(IntPtr userData, uint reqId, IntPtr authReq)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcReq>>(userData);
            tcs.SetResult(new AuthIpcReq(reqId, new AuthReq(Marshal.PtrToStructure<AuthReqNative>(authReq))));
        }

        private static readonly UIntAuthReqCb DelegateOnDecodeIpcReqAuthCb = OnDecodeIpcReqAuthCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(UIntContainersReqCb))]
#endif
        private static void OnDecodeIpcReqContainersCb(IntPtr userData, uint reqId, IntPtr authReq)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcReq>>(userData);
            tcs.SetResult(new ContainersIpcReq(reqId, new ContainersReq(Marshal.PtrToStructure<ContainersReqNative>(authReq))));
        }

        private static readonly UIntContainersReqCb DelegateOnDecodeIpcReqContainersCb = OnDecodeIpcReqContainersCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(UIntByteListCb))]
#endif
        private static void OnDecodeIpcReqUnregisteredCb(IntPtr userData, uint reqId, IntPtr extraData, UIntPtr size)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcReq>>(userData);
            tcs.SetResult(new UnregisteredIpcReq(reqId, extraData, (ulong)size));
        }

        private static readonly UIntByteListCb DelegateOnDecodeIpcReqUnregisteredCb = OnDecodeIpcReqUnregisteredCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultIpcReqErrorCb))]
#endif
        private static void OnFfiResultIpcReqErrorCb(IntPtr userData, IntPtr result, string msg)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcReq>>(userData);
            var ffiResult = Marshal.PtrToStructure<FfiResult>(result);
            tcs.SetResult(new IpcReqError(ffiResult.ErrorCode, ffiResult.Description, msg));
        }

        private static readonly FfiResultStringCb DelegateOnFfiResultIpcReqErrorCb = OnFfiResultIpcReqErrorCb;

        private delegate void FfiResultIpcReqErrorCb(IntPtr userData, IntPtr result, string msg);
    }
}
