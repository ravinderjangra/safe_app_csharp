using SafeApp;
using SafeApp.MockAuthBindings;
using Xunit;

namespace SafeAppTests
{
    [Collection("Misc Tests")]
    public class MiscTest
    {
        [Fact]
        public void IsMockAuthenticationBuildTest()
            => Assert.True(Authenticator.IsMockBuild());

        [Fact]
        public void IsMockSafeAppBuildTest()
            => Assert.True(Session.IsMockBuild());
    }
}
