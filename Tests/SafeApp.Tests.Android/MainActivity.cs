using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.OS;
using UnitTests.HeadlessRunner;
using Xunit.Runners.UI;

namespace SafeAppTests.Android
{
    [Activity(
      Name = "SafeApp.Tests.Android.MainActivity",
      Label = "SafeApp.Tests.Android",
      Icon = "@drawable/icon",
      Theme = "@android:style/Theme.Holo.Light",
      MainLauncher = true,
      ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    // ReSharper disable once UnusedMember.Global
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            var hostIp = Intent.Extras?.GetString("HOST_IP", null);
            var hostPort = Intent.Extras?.GetInt("HOST_PORT", 10500) ?? 10500;

            if (!string.IsNullOrEmpty(hostIp))
            {
                // Run the headless test runner for CI
                Task.Run(() =>
                {
                    return Tests.RunAsync(new TestOptions
                    {
                        NetworkLogHost = hostIp,
                        NetworkLogPort = hostPort,
                        Format = TestResultsFormat.XunitV2
                    });
                });
            }

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            // run the test automatically on app launch
            AutoStart = true;

            // you cannot add more assemblies once calling base
            base.OnCreate(savedInstanceState);
        }
    }
}
