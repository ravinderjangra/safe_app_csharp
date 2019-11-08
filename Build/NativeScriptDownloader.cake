#addin nuget:?package=SharpZipLib
#addin nuget:?package=Cake.Compression

var TAG = "11046de";
var PR_TAG = "310-2";

var S3_DOWNLOAD_BASE_URL = "https://safe-jenkins-build-artifacts.s3.eu-west-2.amazonaws.com/";
var LIB_DIR_NAME = "../SafeApp.AppBindings/NativeLibs/";
var ANDROID_DIR_NAME = $"{LIB_DIR_NAME}Android";
var IOS_DIR_NAME = $"{LIB_DIR_NAME}iOS";
var DESKTOP_DIR_NAME = $"{LIB_DIR_NAME}Desktop";
var Native_DIR = Directory($"{System.IO.Path.GetTempPath()}nativelibs");

var ANDROID_ARMEABI_V7A = "armv7-linux-androideabi";
var ANDROID_x86_64 = "x86_64-linux-android";
var ANDROID_ARCHITECTURES = new string[] {
  ANDROID_ARMEABI_V7A,
  ANDROID_x86_64
};
var IOS_ARCHITECTURES = new string[] {
  "apple-ios"
};
var DESKTOP_ARCHITECTURES = new string[] {
  "x86_64-unknown-linux-gnu",
  "x86_64-apple-darwin",
  "x86_64-pc-windows-gnu"
};
var All_ARCHITECTURES = new string[][] {
  ANDROID_ARCHITECTURES,
  IOS_ARCHITECTURES,
  DESKTOP_ARCHITECTURES
};

enum Environment {
  Android,
  Ios,
  Desktop
}

var varients = new string[] {
  "",
  "dev" 
};

Task("Download-Libs")
  .Does(() => {
    foreach(var item in Enum.GetValues(typeof (Environment)))
    {
      string[] targets = null;
      Information($"\n {item} ");
      switch (item) 
      {
      case Environment.Android:
        targets = ANDROID_ARCHITECTURES;
        break;
      case Environment.Ios:
        targets = IOS_ARCHITECTURES;
        break;
      case Environment.Desktop:
        targets = DESKTOP_ARCHITECTURES;
        break;
      }

      foreach(var target in targets) {
        foreach (var varient in varients)
        {
          var targetDirectory = $"{Native_DIR.Path}/{item}/{target}";
          var zipFileName = varient == "" ? $"{PR_TAG}-safe-ffi-{target}.tar.gz" : $"{PR_TAG}-safe-ffi-{target}-{varient}.tar.gz";
          var zipFileDownloadUrl = $"{S3_DOWNLOAD_BASE_URL}{zipFileName}";
          var zipSavePath = $"{Native_DIR.Path}/{item}/{target}/{zipFileName}";

          if(!DirectoryExists(targetDirectory))
            CreateDirectory(targetDirectory);
          else
          {
            var existingNativeLibs = GetFiles($"{targetDirectory}/*.zip");
            if(existingNativeLibs.Count > 0)
            foreach(var lib in existingNativeLibs) 
            {
              if(!lib.GetFilename().ToString().Contains(PR_TAG))
                DeleteFile(lib.FullPath);
            }
          }

          Information("Downloading : {0}", zipFileName);
          if(!FileExists(zipSavePath))
          {
            DownloadFile(zipFileDownloadUrl, File(zipSavePath));
          }
          else
          {
            Information("File already exists");
          }
        }
      }
    }
  })
  .ReportError(exception => {
    Information(exception.Message);
  });

Task("UnZip-Libs")
  .IsDependentOn("Download-Libs")
  .Does(() => {
    foreach(var item in Enum.GetValues(typeof(Environment))) {
      string[] targets = null;
      var outputDirectory = string.Empty;
      Information($"\n {item} ");
      switch (item) 
      {
      case Environment.Android:
        targets = ANDROID_ARCHITECTURES;
        outputDirectory = ANDROID_DIR_NAME;
        break;
      case Environment.Ios:
        targets = IOS_ARCHITECTURES;
        outputDirectory = IOS_DIR_NAME;
        break;
      case Environment.Desktop:
        targets = DESKTOP_ARCHITECTURES;
        outputDirectory = DESKTOP_DIR_NAME;
        break;
      }

      CleanDirectories(outputDirectory);

      foreach(var target in targets) {
        var zipSourceDirectory = Directory($"{Native_DIR.Path}/{item}/{target}");
        var zipFiles = GetFiles($"{zipSourceDirectory}/*.*");
        foreach(var zip in zipFiles) 
        {
          var filename = zip.GetFilename();
          Information(" Unzipping : " + filename);
          var platformOutputDirectory = new StringBuilder();
          platformOutputDirectory.Append(outputDirectory);
          
          if(filename.ToString().Contains("dev")) 
          {
            platformOutputDirectory.Append("/mock");
          } 
          else 
          {
            platformOutputDirectory.Append("/non-mock");
          }
          
          if(target.Equals(ANDROID_ARMEABI_V7A))
            platformOutputDirectory.Append("/armeabi-v7a");
          else if(target.Equals(ANDROID_x86_64))
            platformOutputDirectory.Append("/x86_64");

          GZipUncompress(zip, platformOutputDirectory.ToString());

          var unZippedFiles = GetFiles($"{platformOutputDirectory.ToString()}/*.*");
          foreach (var file in unZippedFiles)
          {
            MoveFile(file.FullPath, file.FullPath.Replace("safe_ffi", "safe_api"));
          }

          if(target.Contains("darwin") || target.Contains("android") || target.Contains("linux"))
          {
            var aFile = GetFiles(string.Format("{0}/*.a", platformOutputDirectory.ToString()));
            if(aFile.Count > 0)
              DeleteFile(aFile.ToArray()[0].FullPath);
            var rFile = GetFiles(string.Format("{0}/*.rlib", platformOutputDirectory.ToString()));
            if(rFile.Count > 0)
              DeleteFile(rFile.ToArray()[0].FullPath);
            var dFile = GetFiles(string.Format("{0}/*.d", platformOutputDirectory.ToString()));
            if(dFile.Count > 0)
              DeleteFile(dFile.ToArray()[0].FullPath);
          }
        }
      }
    }
  })
  .ReportError(exception => {
    Information(exception.Message);
  });
