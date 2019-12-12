using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SafeApp.Core;
using SafeApp.MockAuthBindings;

namespace SafeApp.Tests
{
    [TestFixture]
    internal class MiscTest
    {
        [Test]
        public void IsMockAuthenticationBuildTest()
        {
#if NON_MOCK_AUTH
            Assert.That(Authenticator.IsMockBuild(), Is.False);
#else
            Assert.That(Authenticator.IsMockBuild(), Is.True);
#endif
        }

        [Test]
        public void IsMockSafeAppBuildTest()
        {
#if NON_MOCK_AUTH
            Assert.That(Session.IsMockBuild(), Is.False);
#else
            Assert.That(Session.IsMockBuild(), Is.True);
#endif
        }
    }
}
