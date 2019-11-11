using System.Runtime.InteropServices;

var SYSTEM_LOCAL_IP = System.Net.Dns.GetHostEntry (System.Net.Dns.GetHostName ())
    .AddressList
    .First (f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    .ToString ();
var VAULT_PORT = 15000;

var VAULT_GITHUB_RELEASE_BASE_URL = "https://github.com/maidsafe/safe_vault/releases/download";
var VAULT_EXE_NAME = "safe_vault";
var VAULT_RELEASE_TAG = "0.19.2";
var VAULT_EXE_DIRECTORY = Directory ($"VaultExecutables");
var VAULT_EXE_Zip_DIRECTORY = Directory ($"{VAULT_EXE_DIRECTORY}/Zips");
var AUTH_CONSOLE_TEST_PROJ_DIR = "../Tests/SafeApp.Tests.AuthConsole/";
var TEST_AUTH_CRED_FILE_DIR = "../Tests";
var TEST_AUTH_CRED_FILE = "../Tests/TestAuthResponse.txt";

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

Task ("Run-Local-Vault")
     .IsDependentOn ("UnZip-Vault-Exe")
    .Does (() => {
        var exeFileName = String.Empty;
        var exeFilePath = String.Empty;

        if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
            exeFileName = VAULT_EXE_NAME;
            exeFilePath = $"{VAULT_EXE_DIRECTORY}/macos/{exeFileName}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
            exeFileName = VAULT_EXE_NAME;
            exeFilePath = $"{VAULT_EXE_DIRECTORY}/linux/{exeFileName}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
            exeFileName = $"{VAULT_EXE_NAME}.exe";
            exeFilePath = $"{VAULT_EXE_DIRECTORY}/windows/{exeFileName}";
        }

        Information (SYSTEM_LOCAL_IP);

        if (!String.IsNullOrWhiteSpace (exeFilePath)) {
            vaultProcess = StartAndReturnProcess (exeFilePath, new ProcessSettings {
                Arguments = $"--ip {SYSTEM_LOCAL_IP} --port {VAULT_PORT} -vvvv"
            });
        }

        System.Threading.Thread.Sleep (200000);
    })
    .ReportError (exception => {
        Information (exception.Message);
    });

Task ("Kill-Local-Vault")
    .Does (() => {
        if (vaultProcess != null) {
            vaultProcess.Kill ();
            Information ("vault killed");
        }
    })
    .ReportError (exception => {
        Information (exception.Message);
    });

Task ("Run-AuthConsole")
    .IsDependentOn ("Restore-NuGet")
    .Does (() => {

        if (FileExists (TEST_AUTH_CRED_FILE))
            DeleteFile (TEST_AUTH_CRED_FILE);

        var dotnetBuildArgument = @"/p:DefineConstants=""NON_MOCK_AUTH""";
        var buildSettings = new DotNetCoreMSBuildSettings () {
            ArgumentCustomization = args => args.Append (dotnetBuildArgument)
        };
        buildSettings.SetConfiguration (configuration);
        DotNetCoreMSBuild (AUTH_CONSOLE_TEST_PROJ_DIR, buildSettings);

        var settings = new DotNetCoreRunSettings
        {
            NoBuild = true
        };

        DotNetCoreRun (AUTH_CONSOLE_TEST_PROJ_DIR, $"{TEST_AUTH_CRED_FILE_DIR}", settings);
    })
    .ReportError (exception => {
        Information (exception.Message);
    });