using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SafeApp.Core;

namespace SafeAuthenticator
{
    /// <summary>
    /// App permissions.
    /// </summary>
    public struct AppPermissions
    {
        /// <summary>
        /// Set true, if the app is allowed to transfer coins.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool TransferCoins;

        /// <summary>
        /// Set true, if the app is allowed to mutate data.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool PerformMutations;

        /// <summary>
        /// Set true, if the app is allowed to get balance.
        /// </summary>
        [MarshalAs(UnmanagedType.U1)]
        public bool GetBalance;
    }

    /// <summary>
    /// Authenticated app information.
    /// </summary>
    public struct AuthedApp
    {
        /// <summary>
        /// Application identifier.
        /// </summary>
        public string Id;

        /// <summary>
        /// Application name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Application provider/vendor (e.g. MaidSafe).
        /// </summary>
        public string Vendor;

        /// <summary>
        /// AppPermissions for the app.
        /// </summary>
        public AppPermissions AppPermissions;

        /// <summary>
        /// The list of container permissions for the app.
        /// </summary>
        public List<ContainerPermissions> Containers;

        /// <summary>
        /// Set true, if the app has a dedicated container for itself.
        /// false otherwise.
        /// </summary>
        public bool OwnContainer;

        internal AuthedApp(AuthedAppNative native)
        {
            Id = native.Id;
            Name = native.Name;
            Vendor = native.Vendor;
            AppPermissions = native.AppPermissions;
            Containers = BindingUtils.CopyToObjectList<ContainerPermissions>(native.ContainersPtr, (int)native.ContainersLen);
            OwnContainer = native.OwnContainer;
        }

        internal AuthedAppNative ToNative()
        {
            return new AuthedAppNative
            {
                Id = Id,
                Name = Name,
                Vendor = Vendor,
                AppPermissions = AppPermissions,
                ContainersPtr = BindingUtils.CopyFromObjectList(Containers),
                ContainersLen = (UIntPtr)(Containers?.Count ?? 0),
                OwnContainer = OwnContainer
            };
        }
    }

    internal struct AuthedAppNative
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Id;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Vendor;
        public AppPermissions AppPermissions;
        public IntPtr ContainersPtr;
        public UIntPtr ContainersLen;
        [MarshalAs(UnmanagedType.U1)]
        public bool OwnContainer;

        internal void Free()
        {
            BindingUtils.FreeList(ref ContainersPtr, ref ContainersLen);
        }
    }
}
