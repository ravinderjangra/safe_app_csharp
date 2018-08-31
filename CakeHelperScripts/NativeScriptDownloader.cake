var TAG = "6be5558";
var ANDROID_DIR_NAME = "SafeApp.AppBindings.Android";
var IOS_DIR_NAME = "SafeApp.AppBindings.iOS";
var DESKTOP_DIR_NAME = "SafeApp.AppBindings.Desktop";

var ANDROID_X86 = "android-x86";
var ANDROID_ARMEABI_V7A = "android-armeabiv7a";
var ANDROID_ARCHITECTURES = new string[] {
  ANDROID_X86,
  ANDROID_ARMEABI_V7A
};
var IOS_ARCHITECTURES = new string[] {
  "ios"
};
var DESKTOP_ARCHITECTURES = new string[] {
  "linux-x64",
  "osx-x64",
  "win-x64"
};
var All_ARCHITECTURES = new string[][] {
  ANDROID_ARCHITECTURES,
  IOS_ARCHITECTURES,
  DESKTOP_ARCHITECTURES
};

var Native_DIR = Directory(string.Concat(System.IO.Path.GetTempPath(), "nativelibs"));
enum Environment {
  Android,
  Ios,
  Desktop
}

Task("Download-Libs")
  .Does(() => {
    foreach(var item in Enum.GetValues(typeof (Environment)))
    {
      string[] targets = null;
      Information(string.Format("\n{0} ", item));
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
        var targetDirectory = string.Format("{0}/{1}/{2}", Native_DIR.Path, item, target);
        var mockZipFilename = string.Format("safe_app-mock-{0}-{1}.zip", TAG, target);
        var nonMockZipFilename = string.Format("safe_app-{0}-{1}.zip", TAG, target);
        var mockZipUrl = string.Format("https://s3.eu-west-2.amazonaws.com/safe-client-libs/{0}", mockZipFilename);
        var nonMockZipUrl = string.Format("https://s3.eu-west-2.amazonaws.com/safe-client-libs/{0}", nonMockZipFilename);
        var mockZipSavePath = string.Format("{0}/{1}/{2}/{3}", Native_DIR.Path, item, target, mockZipFilename);
        var nonMockZipSavePath = string.Format("{0}/{1}/{2}/{3}", Native_DIR.Path, item, target, nonMockZipFilename);

        if(!DirectoryExists(targetDirectory))
          CreateDirectory(targetDirectory);

        Information("Downloading : {0}", mockZipFilename);
        if(!FileExists(mockZipSavePath))
        {
          DownloadFile(mockZipUrl, File(mockZipSavePath));
        }
        else
        {
          Information("File already exists");
        }
        
        Information("Downloading : {0}", nonMockZipFilename);
        if(!FileExists(nonMockZipSavePath))
        {
          DownloadFile(nonMockZipUrl, File(nonMockZipSavePath));
        }
        else
        {
          Information("File already exists");
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
      Information(string.Format("\n {0} ", item));
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

      CleanDirectories(string.Concat(outputDirectory, "/lib"));

      foreach(var target in targets) {
        var zipSourceDirectory = Directory(string.Format("{0}/{1}/{2}", Native_DIR.Path, item, target));
        var zipFiles = GetFiles(string.Format("{0}/*.*", zipSourceDirectory));
        foreach(var zip in zipFiles) 
        {
          var filename = zip.GetFilename();
          Information(" Unzipping : " + filename);
          var platformOutputDirectory = new StringBuilder();
          platformOutputDirectory.Append(outputDirectory);
          platformOutputDirectory.Append("/lib");
          
          if(filename.ToString().Contains("mock")) 
          {
            platformOutputDirectory.Append("/mock");
          } 
          else 
          {
            platformOutputDirectory.Append("/non-mock");
          }

          if(target.Equals(ANDROID_X86))
            platformOutputDirectory.Append("/x86");
          else if(target.Equals(ANDROID_ARMEABI_V7A))
            platformOutputDirectory.Append("/armeabi-v7a");

          Unzip(zip, platformOutputDirectory.ToString());

          if(target.Contains("osx") || target.Contains("android") || target.Contains("linux"))
          {
            var aFile = GetFiles(string.Format("{0}/*.a", platformOutputDirectory.ToString()));
            DeleteFile(aFile.ToArray()[0].FullPath);
          }

        }
      }
    }
  })
  .ReportError(exception => {
    Information(exception.Message);
  });
