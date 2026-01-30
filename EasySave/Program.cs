using Microsoft.Extensions.Configuration;

namespace EasySave;

internal static class Program
{
    private static void Main(string[] args)
    {
        // Récupération de la configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var appSettings = configuration.Get<ApplicationConfiguration>();
    }
}