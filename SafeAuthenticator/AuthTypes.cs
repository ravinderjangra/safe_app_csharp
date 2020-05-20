using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SafeApp.Core;

namespace SafeAuthenticator
{
    public struct AppPermissions
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool TransferCoins;
        [MarshalAs(UnmanagedType.U1)]
        public bool PerformMutations;
        [MarshalAs(UnmanagedType.U1)]
        public bool GetBalance;
    }

    public struct AuthedApp
    {
        public string Id;
        public string Name;
        public string Vendor;
        public AppPermissions AppPermissions;
        public List<ContainerPermissions> Containers;
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
