using Microsoft.Extensions.Configuration;

namespace EasySave;

internal static class Program
{
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Load();
        var appConfig = ApplicationConfiguration.Instance;
    }
}