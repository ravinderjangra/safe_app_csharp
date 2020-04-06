using System.Runtime.InteropServices;

var CLI_GITHUB_RELEASE_BASE_URL = "https://github.com/maidsafe/safe-api/releases/download";
var CLI_EXE_NAME = "safe";
var CLI_RELEASE_TAG = "0.11.0";
var CLI_EXE_DIRECTORY = Directory ($"./CliExecutables");
var CLI_EXE_Zip_DIRECTORY = Directory ($"./{CLI_EXE_DIRECTORY.Path.FullPath}/Zips");
IDictionary<string, string> CLI_DESKTOP_ARCHITECTURES = new Dictionary<string, string> () 
{
    { "linux", "x86_64-unknown-linux-gnu" },
    { "macos", "x86_64-apple-darwin" },
    { "windows", "x86_64-pc-windows-msvc" }
};

Task ("Download-Cli")
    .Does (() => {
        foreach (var item in CLI_DESKTOP_ARCHITECTURES) {
            var targetDirectory = $"{CLI_EXE_Zip_DIRECTORY}/{item.Key}";
            var CLIZipFileName = $"safe-cli-{CLI_RELEASE_TAG}-{item.Value}.zip";
            var CLIZipFileDownloadUrl = $"{CLI_GITHUB_RELEASE_BASE_URL}/{CLI_RELEASE_TAG}/{CLIZipFileName}";
            var zipSavePath = $"{targetDirectory}/{CLIZipFileName}";

            if (!DirectoryExists (targetDirectory)) {
                CreateDirectory (targetDirectory);
            } else {
                var existingCLIExeFiles = GetFiles ($"{targetDirectory}/*.zip");
                if (existingCLIExeFiles.Count > 0)
                    foreach (var file in existingCLIExeFiles) {
                        if (!file.GetFilename ().ToString ().Contains (CLI_RELEASE_TAG))
                            DeleteFile (file.FullPath);
                    }
            }

            Information ("Downloading : {0}", CLIZipFileDownloadUrl);
            Information ("Downloading : {0}", CLIZipFileName);
            if (!FileExists (zipSavePath)) {
                DownloadFile (CLIZipFileDownloadUrl, File (zipSavePath));
            } else {
                Information ("File already exists");
            }
        }
    })
    .ReportError (exception => {
        Information (exception.Message);
    });

Task ("Unzip-Cli")
    .IsDependentOn ("Download-Cli")
    .Does (() => {
        foreach (var item in CLI_DESKTOP_ARCHITECTURES) {
            var zipTargetDirectory = $"{CLI_EXE_Zip_DIRECTORY}/{item.Key}";
            var targetDirectory = $"{CLI_EXE_DIRECTORY}/{item.Key}";

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

Task ("Run-Section")
    .Does (() => {
        var exeFilePath = String.Empty;

         if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
            exeFilePath = $"./{CLI_EXE_DIRECTORY.Path.FullPath}/macos/{CLI_EXE_NAME}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
            exeFilePath = $"./{CLI_EXE_DIRECTORY.Path.FullPath}/linux/{CLI_EXE_NAME}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
            exeFilePath = $"{CLI_EXE_DIRECTORY}/windows/{CLI_EXE_NAME}.exe";
        }

        if (!String.IsNullOrWhiteSpace (exeFilePath)) {
            var fullFilePath = MakeAbsolute(File(exeFilePath));
            Information (fullFilePath);
            if (FileExists(fullFilePath)) {
                StartProcess (fullFilePath, "vault install");
                StartProcess (fullFilePath, "vault run-baby-fleming");
            }
            else {
                throw new Exception($"CLI file doesn't exists. Expected location : {fullFilePath}");
            }
        }

        System.Threading.Thread.Sleep (10000);
    })
    .ReportError (exception => {
        Information (exception.Message);
    });

Task ("Kill-Section")
    .Does (() => {
        var exeFilePath = String.Empty;

         if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
            exeFilePath = $"./{CLI_EXE_DIRECTORY.Path.FullPath}/macos/{CLI_EXE_NAME}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux)) {
            exeFilePath = $"./{CLI_EXE_DIRECTORY.Path.FullPath}/linux/{CLI_EXE_NAME}";
        } else if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
            exeFilePath = $"{CLI_EXE_DIRECTORY}/windows/{CLI_EXE_NAME}.exe";
        }

        if (!String.IsNullOrWhiteSpace (exeFilePath)) {
            var fullFilePath = MakeAbsolute(File(exeFilePath));
            Information (fullFilePath);
            if (FileExists(fullFilePath)) {
                StartProcess (fullFilePath, "vault killall");
            }
            else {
                throw new Exception($"CLI file doesn't exists. Expected location : {fullFilePath}");
            }
        }

        System.Threading.Thread.Sleep (10000);
    })
    .ReportError (exception => {
        Information (exception.Message);
    });
