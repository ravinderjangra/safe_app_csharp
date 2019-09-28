﻿using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace SafeApp.Core
{
    [PublicAPI]
    public struct XorUrlEncoder
    {
        public ulong EncodingVersion;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)AppConstants.XorNameLen)]
        public byte[] Xorname;
        public ulong TypeTag;
        public ulong DataType;
        public ushort ContentType;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Path;
        [MarshalAs(UnmanagedType.LPStr)]
        public string SubNames;
        public ulong ContentVersion;
    }

    public interface ISafeData
    {
    }

    [PublicAPI]
    public struct SafeKey : ISafeData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)AppConstants.XorNameLen)]
        public byte[] Xorname;
        [MarshalAs(UnmanagedType.LPStr)]
        public string ResolvedFrom;
    }

    [PublicAPI]
    public struct Wallet : ISafeData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)AppConstants.XorNameLen)]
        public byte[] Xorname;
        public ulong TypeTag;
        [MarshalAs(UnmanagedType.LPStr)]
        public string Balances;
        public ulong DataType;
        [MarshalAs(UnmanagedType.LPStr)]
        public string ResolvedFrom;
    }

    [PublicAPI]
    public struct FilesContainer : ISafeData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)AppConstants.XorNameLen)]
        public byte[] Xorname;
        public ulong TypeTag;
        public ulong Version;
        [MarshalAs(UnmanagedType.LPStr)]
        public string FilesMap;
        public ulong DataType;
        [MarshalAs(UnmanagedType.LPStr)]
        public string ResolvedFrom;
    }

    [PublicAPI]
    public struct PublishedImmutableData : ISafeData
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)AppConstants.XorNameLen)]
        public byte[] Xorname;
        public byte[] Data;
        public string ResolvedFrom;
        public string MediaType;

        internal PublishedImmutableData(PublishedImmutableDataNative native)
        {
            Xorname = native.Xorname;
            Data = BindingUtils.CopyToByteArray(native.DataPtr, (int)native.DataLen);
            ResolvedFrom = native.ResolvedFrom;
            MediaType = native.MediaType;
        }

        internal PublishedImmutableDataNative ToNative()
        {
            return new PublishedImmutableDataNative
            {
                Xorname = Xorname,
                DataPtr = BindingUtils.CopyFromByteArray(Data),
                DataLen = (UIntPtr)(Data?.Length ?? 0),
                ResolvedFrom = ResolvedFrom,
                MediaType = MediaType
            };
        }
    }

    internal struct PublishedImmutableDataNative
    {
        public byte[] Xorname;
        public IntPtr DataPtr;
        public UIntPtr DataLen;
        [MarshalAs(UnmanagedType.LPStr)]
        public string ResolvedFrom;
        [MarshalAs(UnmanagedType.LPStr)]
        public string MediaType;

        internal void Free()
        {
            BindingUtils.FreeList(ref DataPtr, ref DataLen);
        }
    }

    [PublicAPI]
    public struct SafeDataFetchFailed : ISafeData
    {
        public readonly int Code;
        public readonly string Description;

        public SafeDataFetchFailed(int code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}