using System.Runtime.CompilerServices;

namespace JWTToken.Configuration
{
    public class ServiceLocator
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration => ServiceProvider.GetRequiredService<IConfiguration>();
        public static void SetLocatorProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
