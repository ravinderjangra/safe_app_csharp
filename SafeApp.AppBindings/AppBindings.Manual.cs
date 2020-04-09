#if !NETSTANDARD || __DESKTOP__

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using SafeApp.Core;

#if __IOS__
using ObjCRuntime;
#endif

namespace SafeApp.AppBindings
{
    internal partial class AppBindings
    {
        #region Decode IPC Msg
        public Task<IpcMsg> DecodeIpcMsgAsync(string msg)
        {
            var (task, userData) = BindingUtils.PrepareTask<IpcMsg>();
            DecodeIpcMsgNative(
              msg,
              userData,
              DelegateOnDecodeIpcMsgAuthCb,
              DelegateOnDecodeIpcMsgUnregisteredCb,
              DelegateOnDecodeIpcMsgContainersCb,
              DelegateOnDecodeIpcMsgShareMdataCb,
              DelegateOnDecodeIpcMsgRevokedCb,
              DelegateOnDecodeIpcMsgErrCb);

            return task;
        }

#if __IOS__
        [MonoPInvokeCallback(typeof(UIntAuthGrantedCb))]
#endif
        private static void OnDecodeIpcMsgAuthCb(IntPtr userData, uint reqId, IntPtr authGranted)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
            tcs.SetResult(new AuthIpcMsg(reqId, new AuthGranted(Marshal.PtrToStructure<AuthGrantedNative>(authGranted))));
        }

        private static readonly UIntAuthGrantedCb DelegateOnDecodeIpcMsgAuthCb = OnDecodeIpcMsgAuthCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(UIntByteListCb))]
#endif
        private static void OnDecodeIpcMsgUnregisteredCb(IntPtr userData, uint reqId, IntPtr serialisedCfgPtr, UIntPtr serialisedCfgLen)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
            tcs.SetResult(new UnregisteredIpcMsg(reqId, serialisedCfgPtr, serialisedCfgLen));
        }

        private static readonly UIntByteListCb DelegateOnDecodeIpcMsgUnregisteredCb = OnDecodeIpcMsgUnregisteredCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(UIntCb))]
#endif
        private static void OnDecodeIpcMsgContainersCb(IntPtr userData, uint reqId)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
            tcs.SetResult(new ContainersIpcMsg(reqId));
        }

        private static readonly UIntCb DelegateOnDecodeIpcMsgContainersCb = OnDecodeIpcMsgContainersCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(UIntCb))]
#endif
        private static void OnDecodeIpcMsgShareMdataCb(IntPtr userData, uint reqId)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
            tcs.SetResult(new ShareMDataIpcMsg(reqId));
        }

        private static readonly UIntCb DelegateOnDecodeIpcMsgShareMdataCb = OnDecodeIpcMsgShareMdataCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(NoneCb))]
#endif
        private static void OnDecodeIpcMsgRevokedCb(IntPtr userData)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
            tcs.SetResult(new RevokedIpcMsg());
        }

        private static readonly NoneCb DelegateOnDecodeIpcMsgRevokedCb = OnDecodeIpcMsgRevokedCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultUIntCb))]
#endif
        private static void OnDecodeIpcMsgErrCb(IntPtr userData, IntPtr result, uint reqId)
        {
            var res = Marshal.PtrToStructure<FfiResult>(result);
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<IpcMsg>>(userData);
            tcs.SetException(new IpcMsgException(reqId, res.ErrorCode, res.Description));
        }

        private static readonly FfiResultUIntCb DelegateOnDecodeIpcMsgErrCb = OnDecodeIpcMsgErrCb;

        #endregion

        #region Fetch

        public Task<ISafeData> FetchAsync(IntPtr app, string url, ulong start, ulong end)
        {
            var (task, userData) = BindingUtils.PrepareTask<ISafeData>();
            FetchNative(
              app,
              url,
              userData,
              start,
              end,
              DelegateOnFfiResultPublishedImmutableDataCb,
              DelegateOnFfiResultWalletCb,
              DelegateOnFfiResultSafeKeyCb,
              DelegateOnFfiResultFilesContainerCb,
              DelegateOnFfiFetchFailedCb);
            return task;
        }

        [DllImport(DllName, EntryPoint = "fetch")]
        private static extern void FetchNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            IntPtr userData,
            ulong start,
            ulong end,
            FfiResultPublishedImmutableDataCb oPublished,
            FfiResultWalletCb oWallet,
            FfiResultSafeKeyCb oKeys,
            FfiResultFilesContainerCb oContainer,
            FfiFetchFailedCb oErr);

        public Task<ISafeData> InspectAsync(IntPtr app, string url)
        {
            var (task, userData) = BindingUtils.PrepareTask<ISafeData>();
            InspectNative(
                app,
                url,
                userData,
                DelegateOnFfiResultPublishedImmutableDataCb,
                DelegateOnFfiResultWalletCb,
                DelegateOnFfiResultSafeKeyCb,
                DelegateOnFfiResultFilesContainerCb,
                DelegateOnFfiFetchFailedCb);
            return task;
        }

        [DllImport(DllName, EntryPoint = "inspect")]
        private static extern void InspectNative(
            IntPtr app,
            [MarshalAs(UnmanagedType.LPStr)] string url,
            IntPtr userData,
            FfiResultPublishedImmutableDataCb oPublished,
            FfiResultWalletCb oWallet,
            FfiResultSafeKeyCb oKeys,
            FfiResultFilesContainerCb oContainer,
            FfiFetchFailedCb oErr);

        private delegate void FfiResultPublishedImmutableDataCb(IntPtr userData, IntPtr publishedImmutableData);

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultPublishedImmutableDataCb))]
#endif
        private static void OnFfiResultPublishedImmutableDataCb(IntPtr userData, IntPtr publishedImmutableData)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<ISafeData>>(userData);
            tcs.SetResult(new PublishedImmutableData(Marshal.PtrToStructure<PublishedImmutableDataNative>(publishedImmutableData)));
        }

        private static readonly FfiResultPublishedImmutableDataCb DelegateOnFfiResultPublishedImmutableDataCb = OnFfiResultPublishedImmutableDataCb;

        private delegate void FfiResultWalletCb(IntPtr userData, IntPtr wallet);

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultWalletCb))]
#endif
        private static void OnFfiResultWalletCb(IntPtr userData, IntPtr wallet)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<ISafeData>>(userData);
            tcs.SetResult(new Wallet(Marshal.PtrToStructure<WalletNative>(wallet)));
        }

        private static readonly FfiResultWalletCb DelegateOnFfiResultWalletCb = OnFfiResultWalletCb;

        private delegate void FfiResultSafeKeyCb(IntPtr userData, IntPtr safeKey);

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultSafeKeyCb))]
#endif
        private static void OnFfiResultSafeKeyCb(IntPtr userData, IntPtr safeKey)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<ISafeData>>(userData);
            tcs.SetResult(new SafeKey(Marshal.PtrToStructure<SafeKeyNative>(safeKey)));
        }

        private static readonly FfiResultSafeKeyCb DelegateOnFfiResultSafeKeyCb = OnFfiResultSafeKeyCb;

        private delegate void FfiResultFilesContainerCb(IntPtr userData, IntPtr filesContainer);

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultFilesContainerCb))]
#endif
        private static void OnFfiResultFilesContainerCb(IntPtr userData, IntPtr filesContainer)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<ISafeData>>(userData);
            tcs.SetResult(new FilesContainer(Marshal.PtrToStructure<FilesContainerNative>(filesContainer)));
        }

        private static readonly FfiResultFilesContainerCb DelegateOnFfiResultFilesContainerCb = OnFfiResultFilesContainerCb;

        private delegate void FfiFetchFailedCb(IntPtr userData, IntPtr result);

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiFetchFailedCb))]
#endif
        private static void OnFfiFetchFailedCb(IntPtr userData, IntPtr result)
        {
            var tcs = BindingUtils.FromHandlePtr<TaskCompletionSource<ISafeData>>(userData);
            var ffiResult = Marshal.PtrToStructure<FfiResult>(result);
            tcs.SetResult(new SafeDataFetchFailed(ffiResult.ErrorCode, ffiResult.Description));
        }

        private static readonly FfiFetchFailedCb DelegateOnFfiFetchFailedCb = OnFfiFetchFailedCb;

        #endregion

        #region Connect
        public void ConnectApp(
            string appId,
            string authCredentials,
            Action oDisconnectNotifierCb,
            Action<FfiResult, IntPtr, GCHandle> oCb)
        {
            var userData = BindingUtils.ToHandlePtr(oCb);
            ConnectAppNative(
                appId,
                authCredentials,
                userData,
                DelegateOnAppDisconnectCb,
                DelegateOnFfiResultSafeCb);
        }

        [DllImport(DllName, EntryPoint = "connect_app")]
        private static extern void ConnectAppNative(
            [MarshalAs(UnmanagedType.LPStr)] string appId,
            [MarshalAs(UnmanagedType.LPStr)] string authCredentials,
            IntPtr userData,
            NoneCb oDisconnectNotifierCb,
            FfiResultSafeCb oCb);

        private delegate void FfiResultSafeCb(IntPtr userData, IntPtr result, IntPtr app);

#if __IOS__
        [MonoPInvokeCallback(typeof(FfiResultSafeCb))]
#endif
        private static void OnFfiResultSafeCb(IntPtr userData, IntPtr result, IntPtr app)
        {
            var action = BindingUtils.FromHandlePtr<Action<FfiResult, IntPtr, GCHandle>>(userData, false);
            action(Marshal.PtrToStructure<FfiResult>(result), app, GCHandle.FromIntPtr(userData));
        }

        private static readonly FfiResultSafeCb DelegateOnFfiResultSafeCb = OnFfiResultSafeCb;

#if __IOS__
        [MonoPInvokeCallback(typeof(NoneCb))]
#endif
        private static void OnAppDisconnectCb(IntPtr userData)
        {
            var (action, _) = BindingUtils.FromHandlePtr<(Action, Action<FfiResult, IntPtr, GCHandle>)>(userData, false);
            action();
        }

        private static readonly NoneCb DelegateOnAppDisconnectCb = OnAppDisconnectCb;

        #endregion
    }
}
#endif
