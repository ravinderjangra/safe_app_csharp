using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using UnitTests.HeadlessRunner;
using Xunit.Runner;
using Xunit.Sdk;

namespace SafeApp.Tests.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Foundation.Register("AppDelegate")]
    public partial class AppDelegate : RunnerAppDelegate
    {
        private readonly string _tcpListenHost =
            System.Net.Dns.GetHostEntry(
                System.Net.Dns.GetHostName()).AddressList.First(
                f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();

        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            var testCfg = System.IO.File.ReadAllText("tests.cfg")?.Split(':');
            if (testCfg != null && testCfg.Length > 1)
            {
                var ip = testCfg[0];
                if (int.TryParse(testCfg[1], out var port))
                {
                    // Run the headless test runner for CI
                    Task.Run(() =>
                    {
                        return UnitTests.HeadlessRunner.Tests.RunAsync(new TestOptions
                        {
                            Assemblies = new List<Assembly> { Assembly.GetExecutingAssembly() },
                            NetworkLogHost = ip,
                            NetworkLogPort = port,
                            Format = TestResultsFormat.XunitV2,
                        });
                    });
                }
            }

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());

            // otherwise you need to ensure that the test assemblies will
            // become part of the app bundle
            // AddTestAssembly(typeof(PortableTests).Assembly);
#if false
            // you can use the default or set your own custom writer (e.g. save to web site and tweet it ;-)
#pragma warning disable CS0618 // Type or member is obsolete
            Writer = new TcpTextWriter("10.0.1.2", 10500);
#pragma warning restore CS0618 // Type or member is obsolete

            // start running the test suites as soon as the application is loaded
            AutoStart = true;

            // crash the application (to ensure it's ended) and return to springboard
            TerminateAfterExecution = true;
#endif
            return base.FinishedLaunching(app, options);
        }
    }
}
