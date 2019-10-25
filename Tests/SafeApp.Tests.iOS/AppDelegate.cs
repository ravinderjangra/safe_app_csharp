using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using UnitTests.HeadlessRunner;
using Xamarin.Forms;
using Xunit.Runner;

#pragma warning disable SA1300 // Element should begin with upper-case letter
namespace SafeAppTests.iOS
{
    // ReSharper disable once UnusedMember.Global
    [Register("AppDelegate")]
    public class AppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.Init();

            // Invoke the headless test runner if a config was specified
            var testCfg = System.IO.File.ReadAllText("tests.cfg")?.Split(':');
            if (testCfg != null && testCfg.Length > 1)
            {
                var ip = testCfg[0];
                if (int.TryParse(testCfg[1], out var port))
                {
                    // Run the headless test runner for CI
                    Task.Run(() =>
                    {
                        return Tests.RunAsync(new TestOptions
                        {
                            NetworkLogHost = ip,
                            NetworkLogPort = port,
                            Format = TestResultsFormat.XunitV2
                        });
                    });
                }
            }

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            // run the test automatically on app launch
            AutoStart = true;

            return base.FinishedLaunching(app, options);
        }
    }
}
#pragma warning restore SA1300 // Element should begin with upper-case letter
