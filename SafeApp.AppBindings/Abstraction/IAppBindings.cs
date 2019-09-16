using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SafeApp.Utilities;

// ReSharper disable once CheckNamespace

namespace SafeApp.AppBindings
{
    // ReSharper disable InconsistentNaming
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public partial interface IAppBindings
    {
        Task AppReconnectAsync(IntPtr app);

        Task<string> AppExeFileStemAsync();

        Task AppSetAdditionalSearchPathAsync(string newPath);

        void AppFree(IntPtr app);

        Task AppResetObjectCacheAsync(IntPtr app);

        Task<string> AppContainerNameAsync(string appId);

        bool AppIsMock();

        Task AccessContainerRefreshAccessInfoAsync(IntPtr app);

        Task<List<ContainerPermissions>> AccessContainerFetchAsync(IntPtr app);

        Task<MDataInfo> AccessContainerGetContainerMDataInfoAsync(IntPtr app, string name);

        Task<ulong> CipherOptNewPlaintextAsync(IntPtr app);

        Task<ulong> CipherOptNewSymmetricAsync(IntPtr app);

        Task<ulong> CipherOptNewAsymmetricAsync(IntPtr app, ulong peerEncryptKeyH);

        Task CipherOptFreeAsync(IntPtr app, ulong handle);

        Task<ulong> AppPubSignKeyAsync(IntPtr app);

        Task<(ulong, ulong)> SignGenerateKeyPairAsync(IntPtr app);

        Task<ulong> SignPubKeyNewAsync(IntPtr app, byte[] data);

        Task<byte[]> SignPubKeyGetAsync(IntPtr app, ulong handle);

        Task SignPubKeyFreeAsync(IntPtr app, ulong handle);

        Task<ulong> SignSecKeyNewAsync(IntPtr app, byte[] data);

        Task<byte[]> SignSecKeyGetAsync(IntPtr app, ulong handle);

        Task SignSecKeyFreeAsync(IntPtr app, ulong handle);

        Task<ulong> AppPubEncKeyAsync(IntPtr app);

        Task<(ulong, ulong)> EncGenerateKeyPairAsync(IntPtr app);

        Task<ulong> EncPubKeyNewAsync(IntPtr app, byte[] data);

        Task<byte[]> EncPubKeyGetAsync(IntPtr app, ulong handle);

        Task EncPubKeyFreeAsync(IntPtr app, ulong handle);

        Task<ulong> EncSecretKeyNewAsync(IntPtr app, byte[] data);

        Task<byte[]> EncSecretKeyGetAsync(IntPtr app, ulong handle);

        Task EncSecretKeyFreeAsync(IntPtr app, ulong handle);

        Task<List<byte>> SignAsync(IntPtr app, List<byte> data, ulong signSkH);

        Task<List<byte>> VerifyAsync(IntPtr app, List<byte> signedData, ulong signPkH);

        Task<List<byte>> EncryptAsync(IntPtr app, List<byte> data, ulong publicKeyH, ulong secretKeyH);

        Task<List<byte>> DecryptAsync(IntPtr app, List<byte> data, ulong publicKeyH, ulong secretKeyH);

        Task<List<byte>> EncryptSealedBoxAsync(IntPtr app, List<byte> data, ulong publicKeyH);

        Task<List<byte>> DecryptSealedBoxAsync(IntPtr app, List<byte> data, ulong publicKeyH, ulong secretKeyH);

        Task<List<byte>> Sha3HashAsync(List<byte> data);

        Task<byte[]> GenerateNonceAsync();

        Task<ulong> IDataNewSelfEncryptorAsync(IntPtr app, bool published);

        Task IDataWriteToSelfEncryptorAsync(IntPtr app, ulong seH, List<byte> data);

        Task<byte[]> IDataCloseSelfEncryptorAsync(IntPtr app, ulong seH, ulong cipherOptH, bool published);

        Task<ulong> IDataFetchSelfEncryptorAsync(IntPtr app, byte[] name, bool published);

        Task<ulong> IDataSerialisedSizeAsync(IntPtr app, byte[] name, bool published);

        Task<ulong> IDataSizeAsync(IntPtr app, ulong seH);

        Task<List<byte>> IDataReadFromSelfEncryptorAsync(IntPtr app, ulong seH, ulong fromPos, ulong len);

        Task IDataSelfEncryptorWriterFreeAsync(IntPtr app, ulong handle);

        Task IDataSelfEncryptorReaderFreeAsync(IntPtr app, ulong handle);

        Task<(uint, string)> EncodeAuthReqAsync(ref AuthReq req);

        Task<(uint, string)> EncodeContainersReqAsync(ref ContainersReq req);

        Task<(uint, string)> EncodeUnregisteredReqAsync(List<byte> extraData);

        Task<(uint, string)> EncodeShareMDataReqAsync(ref ShareMDataReq req);

        Task AppInitLoggingAsync(string outputFileNameOverride);

        Task<string> AppOutputLogPathAsync(string outputFileName);

        Task<MDataInfo> MDataInfoNewPrivateAsync(bool mdSeq, byte[] name, ulong typeTag, byte[] secretKey, byte[] nonce);

        Task<MDataInfo> MDataInfoRandomPublicAsync(bool mdSeq, ulong typeTag);

        Task<MDataInfo> MDataInfoRandomPrivateAsync(bool mdSeq, ulong typeTag);

        Task<List<byte>> MDataInfoEncryptEntryKeyAsync(ref MDataInfo info, List<byte> input);

        Task<List<byte>> MDataInfoEncryptEntryValueAsync(ref MDataInfo info, List<byte> input);

        Task<List<byte>> MDataInfoDecryptAsync(ref MDataInfo info, List<byte> input);

        Task<List<byte>> MDataInfoSerialiseAsync(ref MDataInfo info);

        Task<MDataInfo> MDataInfoDeserialiseAsync(List<byte> encoded);

        Task MDataPutAsync(IntPtr app, ref MDataInfo info, ulong permissionsH, ulong entriesH);

        Task<ulong> MDataGetVersionAsync(IntPtr app, ref MDataInfo info);

        Task<ulong> MDataSerialisedSizeAsync(IntPtr app, ref MDataInfo info);

        Task<(List<byte>, ulong)> MDataGetValueAsync(IntPtr app, ref MDataInfo info, List<byte> key);

        Task<ulong> MDataEntriesAsync(IntPtr app, ref MDataInfo info);

        Task<List<MDataKey>> MDataListKeysAsync(IntPtr app, ref MDataInfo info);

        Task<List<MDataValue>> SeqMDataListValuesAsync(IntPtr app, ref MDataInfo info);

        Task MDataMutateEntriesAsync(IntPtr app, ref MDataInfo info, ulong actionsH);

        Task<ulong> MDataListPermissionsAsync(IntPtr app, ref MDataInfo info);

        Task<PermissionSet> MDataListUserPermissionsAsync(IntPtr app, ref MDataInfo info, ulong userH);

        Task MDataSetUserPermissionsAsync(IntPtr app, ref MDataInfo info, ulong userH, ref PermissionSet permissionSet, ulong version);

        Task MDataDelUserPermissionsAsync(IntPtr app, ref MDataInfo info, ulong userH, ulong version);

        Task<ulong> SeqMDataEntriesNewAsync(IntPtr app);

        Task SeqMDataEntriesInsertAsync(IntPtr app, ulong entriesH, List<byte> key, List<byte> value);

        Task<ulong> SeqMDataEntriesLenAsync(IntPtr app, ulong entriesH);

        Task<(List<byte>, ulong)> SeqMDataEntriesGetAsync(IntPtr app, ulong entriesH, List<byte> key);

        Task<List<MDataEntry>> SeqMDataListEntriesAsync(IntPtr app, ulong entriesH);

        Task SeqMDataEntriesFreeAsync(IntPtr app, ulong entriesH);

        Task<ulong> MDataEntryActionsNewAsync(IntPtr app);

        Task MDataEntryActionsInsertAsync(IntPtr app, ulong actionsH, List<byte> key, List<byte> value);

        Task MDataEntryActionsUpdateAsync(IntPtr app, ulong actionsH, List<byte> key, List<byte> value, ulong version);

        Task MDataEntryActionsDeleteAsync(IntPtr app, ulong actionsH, List<byte> key, ulong version);

        Task MDataEntryActionsFreeAsync(IntPtr app, ulong actionsH);

        Task<List<byte>> MDataEncodeMetadataAsync(ref MetadataResponse metadata);

        Task<ulong> MDataPermissionsNewAsync(IntPtr app);

        Task<ulong> MDataPermissionsLenAsync(IntPtr app, ulong permissionsH);

        Task<PermissionSet> MDataPermissionsGetAsync(IntPtr app, ulong permissionsH, ulong userH);

        Task<List<UserPermissionSet>> MDataListPermissionSetsAsync(IntPtr app, ulong permissionsH);

        Task MDataPermissionsInsertAsync(IntPtr app, ulong permissionsH, ulong userH, ref PermissionSet permissionSet);

        Task MDataPermissionsFreeAsync(IntPtr app, ulong permissionsH);

        Task<(File, ulong)> DirFetchFileAsync(IntPtr app, ref MDataInfo parentInfo, string fileName);

        Task DirInsertFileAsync(IntPtr app, ref MDataInfo parentInfo, string fileName, ref File file);

        Task<ulong> DirUpdateFileAsync(IntPtr app, ref MDataInfo parentInfo, string fileName, ref File file, ulong version);

        Task<ulong> DirDeleteFileAsync(IntPtr app, ref MDataInfo parentInfo, string fileName, bool published, ulong version);

        Task<ulong> FileOpenAsync(IntPtr app, ref MDataInfo parentInfo, ref File file, ulong openMode);

        Task<ulong> FileSizeAsync(IntPtr app, ulong fileH);

        Task<List<byte>> FileReadAsync(IntPtr app, ulong fileH, ulong position, ulong len);

        Task FileWriteAsync(IntPtr app, ulong fileH, List<byte> data);

        Task<File> FileCloseAsync(IntPtr app, ulong fileH);

        Task TestSimulateNetworkDisconnectAsync(IntPtr app);
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
