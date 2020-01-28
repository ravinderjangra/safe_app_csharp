using System.Runtime.InteropServices;

var VAULT_GITHUB_RELEASE_BASE_URL = "https://github.com/maidsafe/safe_vault/releases/download";
var VAULT_EXE_NAME = "safe_vault";
var VAULT_RELEASE_TAG = "0.20.1";
var VAULT_EXE_DIRECTORY = Directory ($"./VaultExecutables");
var VAULT_EXE_Zip_DIRECTORY = Directory ($"./{VAULT_EXE_DIRECTORY.Path.FullPath}/Zips");
IDictionary<string, string> VAULT_DESKTOP_ARCHITECTURES = new Dictionary<string, string> () 
{
    { "linux", "x86_64-unknown-linux-musl" },
    { "macos", "x86_64-apple-darwin" },
    { "windows", "x86_64-pc-windows-gnu" }
};
IProcess vaultProcess;

Task ("Download-Vault-Exe")
    .Does (() => {
        foreach (var item in VAULT_DESKTOP_ARCHITECTURES) {
            var targetDirectory = $"{VAULT_EXE_Zip_DIRECTORY}/{item.Key}";
            var vaultZipFileName = $"{VAULT_EXE_NAME}-{VAULT_RELEASE_TAG}-{item.Value}.zip";
            var vaultZipFileDownloadUrl = $"{VAULT_GITHUB_RELEASE_BASE_URL}/{VAULT_RELEASE_TAG}/{vaultZipFileName}";
            var zipSavePath = $"{targetDirectory}/{vaultZipFileName}";

            if (!DirectoryExists (targetDirectory)) {
                CreateDirectory (targetDirectory);
            } else {
                var existingVaultExeFiles = GetFiles ($"{targetDirectory}/*.zip");
                if (existingVaultExeFiles.Count > 0)
                    foreach (var file in existingVaultExeFiles) {
                        if (!file.GetFilename ().ToString ().Contains (VAULT_RELEASE_TAG))
                            DeleteFile (file.FullPath);
                    }
            }

            Information ("Downloading : {0}", vaultZipFileName);
            if (!FileExists (zipSavePath)) {
                DownloadFile (vaultZipFileDownloadUrl, File (zipSavePath));
            } else {
                Information ("File already exists");
            }
        }
    })
    .ReportError (exception => {
        Information (exception.Message);
    });

Task ("UnZip-Vault-Exe")
    .IsDependentOn ("Download-Vault-Exe")
    .Does (() => {
        foreach (var item in VAULT_DESKTOP_ARCHITECTURES) {
            var zipTargetDirectory = $"{VAULT_EXE_Zip_DIRECTORY}/{item.Key}";
            var targetDirectory = $"{VAULT_EXE_DIRECTORY}/{item.Key}";

            if (!DirectoryExists (targetDirectory)) {
                CreateDirectory (targetDirectory);
            }

            CleanDirectory (targetDirectory);

            var zipFiles = GetFiles ($"{zipTargetDirectory}/*.*");
            foreach (var zip in zipFiles) {
                var filename = zip.GetFilename ();
                Information (" Unzipping : " + filename);
                Unzip (zip, targetDirectory);
            }
        }
    })
    .ReportError (exception => {
        Information (exception.Message);
    });

Task ("Run-Vault")
    .Does (() => {
        var exeFilePath = String.Empty;

         if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
            exeFilePath = $"./{VAULT_EXE_DIRECTORY.Path.FullPath}/macos/{VAULT_EXE_NAME}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
            exeFilePath = $"./{VAULT_EXE_DIRECTORY.Path.FullPath}/linux/{VAULT_EXE_NAME}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
            exeFilePath = $"{VAULT_EXE_DIRECTORY}/windows/{VAULT_EXE_NAME}.exe";
        }

        if (!String.IsNullOrWhiteSpace (exeFilePath)) {
            var fullFilePath = MakeAbsolute(File(exeFilePath));
            Information (fullFilePath);
            if (FileExists(fullFilePath)) {
                StartProcess (fullFilePath);
            }
            else {
                throw new Exception($"Vault file doesn't exists. Expected location : {fullFilePath}");
            }
        }

        System.Threading.Thread.Sleep (20000);
    })
    .ReportError (exception => {
        Information (exception.Message);
    });
