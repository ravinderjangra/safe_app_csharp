#tool nuget:?package=JetBrains.ReSharper.CommandLineTools&version=2018.1.3
#addin Cake.Issues
#addin Cake.Issues.InspectCode

var logPath = @"resharper-clt-output.xml";
var buildDirectory = Directory(".");

Task("Analyze-Project")
  .WithCriteria(IsRunningOnWindows())
  .IsDependentOn("Restore-Nuget")
  .Does(() =>
  {
    var settings = new InspectCodeSettings() {
      OutputFile = logPath
    };
    InspectCode(solutionFile.Path, settings);
  });

Task("Analyze-Project-Report")
  .WithCriteria(IsRunningOnWindows())
  .IsDependentOn("Analyze-Project")
  .Does(() =>
  {
    var issues = ReadIssues(
      InspectCodeIssuesFromFilePath(logPath),
      buildDirectory);
    
    if(issues.Count()>0) {
      foreach (var item in issues)
      {
        var issueMessage = $"Priority: {item.PriorityName}, Details: {item.Message}, Line: {item.Line}, File: {item.AffectedFileRelativePath}";
        if(AppVeyor.IsRunningOnAppVeyor)
          AppVeyor.AddMessage(item.Message,  AppVeyorMessageCategoryType.Error, issueMessage);
        else
          Information(issueMessage);
      }
      if(AppVeyor.IsRunningOnAppVeyor)
        throw new Exception("Build Failed : InspectCode issues found. Check message tab for details.");
    }
    else {
      if(AppVeyor.IsRunningOnAppVeyor)
        AppVeyor.AddMessage("No code issues.",  AppVeyorMessageCategoryType.Information);
      else
        Information("InspectCode : No code issues.");
    }
  });
