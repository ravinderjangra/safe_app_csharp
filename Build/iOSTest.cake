#load "Utility.cake"

#addin nuget:?package=Cake.AppleSimulator&Version=0.1.0
#addin nuget:?package=Cake.FileHelpers
#addin "Cake.Powershell"

var IOS_TEST_PROJ_DIR = "../Tests/SafeApp.Tests.iOS/";
var IOS_SIM_NAME = EnvironmentVariable("IOS_SIM_NAME") ?? "iPhone X";
var IOS_SIM_RUNTIME = EnvironmentVariable("IOS_SIM_RUNTIME") ?? "iOS 12.2";
var IOS_TEST_PROJ = $"{IOS_TEST_PROJ_DIR}SafeApp.Tests.iOS.csproj";
var IOS_BUNDLE_ID = "net.maidsafe.SafeApp.Tests.iOS";
var IOS_IPA_PATH = $"{IOS_TEST_PROJ_DIR}bin/iPhoneSimulator/Release/SafeApp.Tests.iOS.app";
var IOS_TESTS_RESULT_PATH = $"{IOS_TEST_PROJ_DIR}iOSTestResult.xml";

var IOS_TCP_LISTEN_HOST = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.First(f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
var IOS_TCP_LISTEN_PORT = 10500;

Task("Build-iOS-Test-Project")
    .IsDependentOn("Restore-NuGet")
    .Does(() => {
    
    // Setup the test listener config to be built into the app
    // Information("")
    var configFilePath = System.IO.Path.Combine(IOS_TEST_PROJ_DIR, "tests.cfg");// (new FilePath(IOS_TEST_PROJ_DIR)).GetDirectory().CombineWithFilePath("tests.cfg");
    var configFileContent = $"{IOS_TCP_LISTEN_HOST}:{IOS_TCP_LISTEN_PORT}";
    Information("Location:" + configFilePath);
    Information("Config data:" + configFileContent);
    FileWriteText(configFilePath, configFileContent);
    
    // Nuget restore
    MSBuild (IOS_TEST_PROJ, c => {
        c.Configuration = "Release";
        c.Targets.Clear();
        c.Targets.Add("Restore");
    });

    // Build the project (with ipa)
    MSBuild(IOS_TEST_PROJ, c =>
    {
        c.Configuration = "Release";
        c.Properties["Platform"] = new List<string> { "iPhoneSimulator" };
        c.Properties["BuildIpa"] = new List<string> { "true" };
        c.Properties["ContinuousIntegrationBuild"] = new List<string> { "false" };
        c.Targets.Clear();
        c.Targets.Add("Rebuild");
        c.SetVerbosity(Verbosity.Diagnostic);
    });
});

Task("Run-iOS-Tests")
    .IsDependentOn("Build-iOS-Test-Project")
    .Does(() => {
    
    var sims = ListAppleSimulators();

    foreach (var s in sims)
    {
        Information("Info: {0} ({1} - {2} - {3})", s.Name, s.Runtime, s.UDID, s.Availability);
    }

    // Look for a matching simulator on the system
    var sim = sims.First (s => s.Name == IOS_SIM_NAME && s.Runtime == IOS_SIM_RUNTIME);

    // Boot the simulator
    Information("Booting: {0} ({1} - {2})", sim.Name, sim.Runtime, sim.UDID);
    if (!sim.State.ToLower().Contains("booted"))
        BootAppleSimulator(sim.UDID);

    // Wait for it to be booted
    for (int i = 0; i < 100; i++)
    {
        if (ListAppleSimulators().Any(s => s.UDID == sim.UDID && s.State.ToLower().Contains("booted")))
        {
            break;
        }
        System.Threading.Thread.Sleep(1000);
    }

    // Install the IPA that was previously built
    var ipaPath = new FilePath(IOS_IPA_PATH);
    Information("Installing: {0}", ipaPath);
    InstalliOSApplication(sim.UDID, MakeAbsolute(ipaPath).FullPath);

    // Start our Test Results TCP listener
    Information("Started TCP Test Results Listener on port: {0}", IOS_TCP_LISTEN_PORT);
    var tcpListenerTask = DownloadTcpTextAsync(IOS_TCP_LISTEN_HOST, IOS_TCP_LISTEN_PORT, IOS_TESTS_RESULT_PATH);

    // Launch the IPA
    Information("Launching: {0}", IOS_BUNDLE_ID);
    LaunchiOSApplication(sim.UDID, IOS_BUNDLE_ID);

    // Wait for the TCP listener to get results
    Information("Waiting for tests...");
    tcpListenerTask.Wait();

    // Close up simulators
    Information("Closing Simulator");
    ShutdownAllAppleSimulators();
})
.ReportError(exception => {
    Information(exception.Message);
});
