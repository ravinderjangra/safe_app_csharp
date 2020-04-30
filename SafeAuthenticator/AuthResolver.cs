using System;
using System.Threading;

namespace SafeAuthenticator
{
    internal static class AuthResolver
    {
#if !NETSTANDARD
        private static readonly Lazy<IAuthBindings> Implementation = new Lazy<IAuthBindings>(
          CreateBindings,
          LazyThreadSafetyMode.PublicationOnly);

        private static IAuthBindings CreateBindings()
        {
            return new AuthBindings();
        }
#endif

        public static IAuthBindings Current
        {
            get
            {
#if NETSTANDARD
                throw NotImplementedInReferenceAssembly();
#else
                return Implementation.Value;
#endif
            }
        }

        private static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException(
              "Please ensure you have SAFE_APP_MOCK defined in the application project as well. " +
              "You should also have a reference to the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}
